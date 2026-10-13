import http from "node:http";

const PORT = Number(process.env.PORT ?? 5156);
const PASSWORD = "meone";

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
    { id: 1, registration: 1001, name: "Ana Souza", email: "ana@grupomultilaser.com.br", department: "ti" },
    { id: 2, registration: 1002, name: "Bruno Lima", email: null, department: "rh" },
    { id: 3, registration: 1003, name: "Carla Dias", email: "carla@grupomultilaser.com.br", department: "qualidade" },
  ],
};

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

function makeToken(registration) {
  const payload = {
    sub: "1",
    name: "USUARIO DE TESTE",
    registration: String(registration),
    department: "ti",
    roles: ["master"],
    applications: [],
  };
  return `mock.${Buffer.from(JSON.stringify(payload)).toString("base64url")}.sig`;
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
    const [, , resource, segment] = url.pathname.split("/").filter(Boolean);

    let body = {};
    if (req.method === "POST" || req.method === "PUT") {
      const chunks = [];
      for await (const chunk of req) chunks.push(chunk);
      try {
        body = JSON.parse(Buffer.concat(chunks).toString() || "{}");
      } catch {
        body = {};
      }
    }

    console.log(req.method, url.pathname + url.search);

    if (url.pathname === "/__mode") {
      mode = url.searchParams.get("m") ?? "ok";
      return send(res, 200, { mode });
    }

    if (resource === "auth" && req.method === "POST" && !segment) {
      return body.password === PASSWORD
        ? send(res, 200, {
            token: makeToken(body.registration),
            refreshToken: "refresh",
            refreshExpires: new Date(Date.now() + 24 * 3600 * 1000).toISOString(),
          })
        : fail(res, 401, "Não foi possível autenticar", "Matrícula ou senha inválida.");
    }

    if (mode !== "ok" && failureModes[mode]) return failureModes[mode](res);

    const anonymousRead = resource === "departments" && req.method === "GET";
    if (!anonymousRead && !req.headers.authorization?.startsWith("Bearer mock.")) {
      return fail(res, 401, "Não autenticado", "Token ausente ou inválido.");
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
