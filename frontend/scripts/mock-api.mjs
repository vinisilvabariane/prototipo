import http from "node:http";
import { createInstructionMock } from "./mock-instructions.mjs";
import { createHelpdeskMock } from "./mock-helpdesk.mjs";
import { createOeeMock } from "./mock-oee.mjs";
import { createProductionMock } from "./mock-production.mjs";
import { createSupplierMock } from "./mock-suppliers.mjs";
import { createOrgChartMock } from "./mock-org-chart.mjs";
import { createWorkflowMock } from "./mock-workflows.mjs";

const PORT = Number(process.env.PORT ?? 5156);
const PASSWORD = "meone";
const TOKEN_TTL_SECONDS = Number(process.env.TOKEN_TTL ?? 900);

const db = {
  departments: [
    { id: 1, name: "ti" },
    { id: 2, name: "rh" },
    { id: 3, name: "qualidade" },
  ],
  roles: [
    { id: 1, name: "master", description: "Administração" },
    { id: 2, name: "operador", description: "Apontamento de produção" },
  ],
  users: [
    { id: 1, registration: 1001, name: "Ana Souza", email: "ana@exemplo.com", department: "ti" },
    { id: 2, registration: 1002, name: "Bruno Lima", email: null, department: "rh" },
    { id: 3, registration: 1003, name: "Carla Dias", email: "carla@exemplo.com", department: "qualidade" },
  ],
};

const handleWorkflows = createWorkflowMock(db.users);
const handleOrgChart = createOrgChartMock(db.users, db.departments);
const handleInstructions = createInstructionMock(db.users);
const handleSuppliers = createSupplierMock(db.users, db.departments);
const handleProduction = createProductionMock(db.users);
const handleHelpdesk = createHelpdeskMock(db.users, handleProduction.data);
const handleOee = createOeeMock(db.users, handleProduction.data, handleHelpdesk.data);

let nextId = 100;
let mode = "ok";

const rules = {
  users: (b) => (!b.name?.trim() ? "O nome é obrigatório." : !b.registration ? "A matrícula é obrigatória." : null),
  roles: (b) => (!b.name?.trim() ? "O nome do perfil é obrigatório." : null),
  departments: (b) => (!b.name?.trim() ? "O nome do departamento é obrigatório." : null),
};

const send = (res, status, body) => {
  res.writeHead(status, { "Content-Type": "application/json" });
  res.end(body === undefined ? undefined : JSON.stringify(body));
};

const fail = (res, status, error, details) => send(res, status, { error, details });

const refreshTokens = new Map();
let refreshCounter = 0;

function makeToken(registration) {
  const user = db.users.find((candidate) => candidate.registration === Number(registration)) ?? db.users[0];
  const payload = {
    sub: String(user.id),
    name: user.name,
    registration: String(registration),
    department: user.department,
    roles: ["master"],
    applications: [],
    exp: Math.floor(Date.now() / 1000) + TOKEN_TTL_SECONDS,
  };
  return `mock.${Buffer.from(JSON.stringify(payload)).toString("base64url")}.sig`;
}

function issueSession(registration) {
  const refreshToken = `refresh-${++refreshCounter}`;
  refreshTokens.set(refreshToken, registration);
  return {
    token: makeToken(registration),
    refreshToken,
    refreshExpires: new Date(Date.now() + 24 * 3600 * 1000).toISOString(),
  };
}

function actorFrom(header) {
  try {
    return Number(JSON.parse(Buffer.from(header.split(".")[1], "base64url").toString("utf8")).sub);
  } catch {
    return 0;
  }
}

function tokenIsValid(header) {
  if (!header?.startsWith("Bearer mock.")) return false;
  try {
    const payload = JSON.parse(Buffer.from(header.split(".")[1], "base64url").toString("utf8"));
    return typeof payload.exp !== "number" || payload.exp * 1000 > Date.now();
  } catch {
    return false;
  }
}

const failureModes = {
  "503": (res) => fail(res, 503, "Serviço indisponível", "O banco de dados não respondeu."),
  "403": (res) => fail(res, 403, "Acesso negado", "Apenas master pode fazer isso."),
  "500": (res) => fail(res, 500, "Erro interno", "NullReferenceException"),
  html: (res) => {
    res.writeHead(200, { "Content-Type": "text/html" });
    res.end("<html>oops</html>");
  },
  problem: (res) =>
    send(res, 400, {
      title: "One or more validation errors occurred.",
      errors: { "$.registration": ["The JSON value could not be converted to System.UInt32."] },
    }),
  slow: () => {},
};

http
  .createServer(async (req, res) => {
    const url = new URL(req.url, "http://localhost");
    const segments = url.pathname.split("/").filter(Boolean);
    const [, , resource, segment, third] = segments;
    const contentType = req.headers["content-type"] ?? "";

    let body = {};
    let raw = Buffer.alloc(0);
    if (req.method === "POST" || req.method === "PUT") {
      const chunks = [];
      for await (const chunk of req) chunks.push(chunk);
      raw = Buffer.concat(chunks);
      if (!contentType.startsWith("multipart/")) {
        try {
          body = JSON.parse(raw.toString() || "{}");
        } catch {
          body = {};
        }
      }
    }

    console.log(req.method, url.pathname + url.search);

    if (url.pathname === "/__mode") {
      mode = url.searchParams.get("m") ?? "ok";
      return send(res, 200, { mode });
    }

    if (resource === "auth" && req.method === "POST" && !segment) {
      return body.password === PASSWORD
        ? send(res, 200, issueSession(body.registration))
        : fail(res, 401, "Não foi possível autenticar", "Matrícula ou senha inválida.");
    }

    if (resource === "auth" && segment === "refresh" && req.method === "POST") {
      const registration = refreshTokens.get(body.refreshToken);
      if (registration === undefined) {
        return fail(res, 400, "Não foi possível renovar a sessão", "Refresh token inválido ou expirado.");
      }
      refreshTokens.delete(body.refreshToken);
      return send(res, 200, issueSession(registration));
    }

    if (mode !== "ok" && failureModes[mode]) return failureModes[mode](res);

    const anonymousRead = resource === "departments" && req.method === "GET";
    if (!anonymousRead && !tokenIsValid(req.headers.authorization)) {
      return fail(res, 401, "Não autenticado", "Token ausente, inválido ou expirado.");
    }

    if (resource === "users" && segment === "roles" && req.method === "GET") {
      return send(res, 200, db.roles.filter((role) => role.name === (Number(third) === 1 ? "master" : "operador")));
    }

    if (resource === "workflows") {
      return handleWorkflows({ req, res, url, segments, actor: actorFrom(req.headers.authorization), json: body, raw, contentType, send, fail });
    }

    if (resource === "production") {
      return handleProduction({ req, res, url, segments, actor: actorFrom(req.headers.authorization), json: body, send, fail });
    }

    if (resource === "helpdesk") {
      return handleHelpdesk({ req, res, url, segments, actor: actorFrom(req.headers.authorization), json: body, send, fail });
    }

    if (resource === "oee") {
      return handleOee({ req, res, url, segments, actor: actorFrom(req.headers.authorization), json: body, send, fail });
    }

    if (resource === "suppliers") {
      return handleSuppliers({ req, res, url, segments, actor: actorFrom(req.headers.authorization), json: body, raw, contentType, send, fail });
    }

    if (resource === "work-instructions") {
      return handleInstructions({ req, res, url, segments, actor: actorFrom(req.headers.authorization), raw, contentType, send, fail });
    }

    if (resource === "org-chart") {
      return handleOrgChart({ req, res, url, segments, actor: actorFrom(req.headers.authorization), json: body, send, fail });
    }

    const list = db[resource];
    if (!list) return fail(res, 404, "Rota inexistente", url.pathname);

    if (req.method === "GET" && (segment === "all" || (resource === "departments" && !segment))) {
      return send(res, 200, list);
    }

    if (req.method === "GET" && !segment) {
      const page = Number(url.searchParams.get("page") ?? 0);
      const size = Number(url.searchParams.get("pageSize") ?? 10);
      const search = (url.searchParams.get("search") ?? "").toLowerCase();
      const found = list.filter((item) => !search || item.name.toLowerCase().includes(search));
      return send(res, 200, { items: found.slice(page * size, page * size + size), totalCount: found.length, page, pageSize: size });
    }

    if (req.method === "POST" && !segment) {
      const error = rules[resource](body);
      if (error) return fail(res, 400, "Falha ao criar", error);
      const { password, ...rest } = body;
      const item = { id: ++nextId, ...rest };
      list.push(item);
      return send(res, 201, item);
    }

    const item = list.find((entry) => String(entry.id) === segment);
    if (!item) return fail(res, 404, "Não encontrado", "Registro inexistente.");

    if (req.method === "GET") return send(res, 200, item);

    if (req.method === "PUT") {
      const error = rules[resource](body);
      if (error) return fail(res, 400, "Falha ao atualizar", error);
      Object.assign(item, body);
      return send(res, 204);
    }

    if (req.method === "DELETE") {
      list.splice(list.indexOf(item), 1);
      return send(res, 204);
    }

    fail(res, 404, "Rota inexistente", url.pathname);
  })
  .listen(PORT, () => {
    console.log(`API de teste do MeOne em http://localhost:${PORT}`);
    console.log(`Login: qualquer matrícula (ex.: 1001) e senha "${PASSWORD}"`);
  });
