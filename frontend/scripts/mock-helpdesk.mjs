const MINUTE = 60_000;
const HOUR = 3_600_000;
const ICONS = ["support", "build", "bolt", "memory", "inventory", "bug", "code", "science", "engineering", "settings", "report", "warning", "shipping", "handyman", "precision", "quality"];
const FIELD_TYPES = ["text", "textArea", "number", "select", "cardRadio", "productionSpot", "lineStop", "defects"];
const TEMPLATE_TYPES = ["text", "textArea", "number", "select", "cardRadio"];
const iso = (ms) => new Date(ms).toISOString();
const isChoice = (type) => type === "select" || type === "cardRadio";
const isSpecial = (type) => ["productionSpot", "lineStop", "defects"].includes(type);

const ROLES = [
  { id: 1, name: "master", description: "Administrador" },
  { id: 4, name: "operador", description: "Operador" },
  { id: 5, name: "qualidade", description: "Qualidade" },
  { id: 6, name: "manutencao", description: "Manutenção" },
  { id: 7, name: "engenharia-processos", description: "Engenharia de processos" },
  { id: 13, name: "gestor-helpdesk", description: "Gestor do help desk" },
];

function checkForm(form, resolution) {
  const fields = form?.fields ?? [];
  if (fields.length > 60) return "O formulário pode ter no máximo 60 campos.";
  const seen = new Map();
  const specials = new Set();

  for (const field of fields) {
    if (!/^[a-z][a-z0-9_]*$/.test(field.id ?? "") || field.id.length > 50) return "O identificador do campo deve começar com letra minúscula e usar só letras minúsculas, números e _.";
    if (!String(field.label ?? "").trim()) return "Todo campo precisa de um título.";
    if (!FIELD_TYPES.includes(field.type)) return "Tipo de campo inválido.";
    if (!isChoice(field.type)) {
      if ((field.options ?? []).length > 0) return "Só lista suspensa e cards têm opções.";
    } else {
      const options = field.options ?? [];
      if (options.length === 0 || options.length > 30) return "Lista suspensa e cards precisam de ao menos uma opção.";
      const values = new Set();
      for (const option of options) {
        if (!String(option.value ?? "").trim() || !String(option.label ?? "").trim()) return "Toda opção precisa de valor e de texto.";
        if (values.has(option.value)) return "Há duas opções com o mesmo valor no mesmo campo.";
        values.add(option.value);
      }
    }
    if (seen.has(field.id)) return "Há dois campos com o mesmo identificador.";
    if (isSpecial(field.type)) {
      if (resolution) return "O formulário de resposta não aceita localização, parada de linha nem defeitos.";
      if (specials.has(field.type)) return "Localização, parada de linha e defeitos podem aparecer uma única vez cada.";
      specials.add(field.type);
    }
    seen.set(field.id, field);
    const dependency = field.dependsOn;
    if (dependency) {
      const parent = seen.get(dependency.fieldId);
      if (!parent || parent === field || !isChoice(parent.type)) return "Um campo condicional precisa depender de uma lista ou cards que vêm antes dele.";
      if (!dependency.values?.length || dependency.values.some((value) => !parent.options.some((option) => option.value === value))) return "Um campo condicional usa uma opção que não existe no campo de que depende.";
    }
  }

  if ((specials.has("lineStop") || specials.has("defects")) && !specials.has("productionSpot")) return "Parada de linha e defeitos precisam de um campo de localização no mesmo formulário.";
  return null;
}

function visible(fields, answers) {
  const shown = [];
  const ids = new Set();
  for (const field of fields) {
    const dependency = field.dependsOn;
    if (dependency && (!ids.has(dependency.fieldId) || !dependency.values.includes(answers[dependency.fieldId]))) continue;
    shown.push(field);
    ids.add(field.id);
  }
  return shown;
}

function normalize(form, raw, now) {
  const provided = Object.fromEntries(Object.entries(raw ?? {}).filter(([, value]) => String(value ?? "").trim() !== "").map(([key, value]) => [key, String(value).trim()]));
  const answers = {};

  for (const field of visible(form.fields, provided)) {
    const value = provided[field.id];
    if (!value) {
      if (field.required) return { problem: `Responda "${field.label}".` };
      continue;
    }
    const bad = (why) => ({ problem: `"${field.label}": ${why}` });
    if (field.type === "text" && value.length > 500) return bad("use no máximo 500 caracteres.");
    if (field.type === "textArea" && value.length > 4000) return bad("use no máximo 4000 caracteres.");
    if (field.type === "number" && !Number.isFinite(Number(value.replace(",", ".")))) return bad("informe um número válido.");
    if (isChoice(field.type) && !field.options.some((option) => option.value === value)) return bad("escolha uma das opções.");
    if (field.type === "productionSpot" && !(Number(value) > 0)) return bad("selecione a linha.");
    if (field.type === "lineStop" && !["stopped", "running"].includes(value)) return bad("informe se a linha está parada.");
    if (field.type === "defects" && !/^\d+$/.test(value)) return bad("informe um número inteiro de 0 a 100000.");
    if (field.type === "defects" && Number(value) > 100000) return bad("informe um número inteiro de 0 a 100000.");
    answers[field.id] = value;

    if (field.type === "productionSpot" && provided[`${field.id}_post`]) {
      if (!(Number(provided[`${field.id}_post`]) > 0)) return bad("selecione o posto.");
      answers[`${field.id}_post`] = provided[`${field.id}_post`];
    }

    if (field.type === "lineStop" && value === "stopped") {
      const time = provided[`${field.id}_time`] ?? iso(now);
      const at = new Date(time).getTime();
      if (Number.isNaN(at)) return bad("informe um horário válido.");
      if (at > now + 5 * MINUTE) return bad("o horário da parada não pode estar no futuro.");
      if (now - at > 7 * 24 * HOUR) return bad("o horário da parada é antigo demais (máximo 7 dias).");
      answers[`${field.id}_time`] = iso(at);
    }
  }

  return { answers };
}

export function createHelpdeskMock(users, productionData) {
  let counter = 5000;
  const next = () => ++counter;
  const now = () => Date.now();
  const nameOf = (id) => users.find((user) => user.id === id)?.name ?? "";

  const sectors = [];
  const templates = [];
  const tickets = [];

  const field = (id, type, label, extra = {}) => ({ id, type, label, required: true, placeholder: null, options: [], dependsOn: null, templateId: null, ...extra });
  const option = (value, label, icon = null, color = null) => ({ value, label, icon, color });

  sectors.push({
    id: next(),
    name: "Manutenção",
    description: "Quebras, ajustes e paradas de máquina.",
    colorHex: "#2547D0",
    icon: "build",
    badges: ["Urgente"],
    active: true,
    externalUrl: null,
    form: {
      fields: [
        field("local", "productionSpot", "Onde está o problema?"),
        field("parada", "lineStop", "A linha está parada?"),
        field("tipo", "cardRadio", "Que tipo de problema?", { options: [option("mecanico", "Mecânico", "build", "#C26A00"), option("eletrico", "Elétrico", "bolt", "#6E1FE8"), option("outro", "Outro", "report", "#4B5563")] }),
        field("componente", "select", "Qual componente?", { options: [option("correia", "Correia"), option("rolamento", "Rolamento"), option("motor", "Motor")], dependsOn: { fieldId: "tipo", values: ["mecanico"] } }),
        field("queda_energia", "cardRadio", "Houve queda de energia?", { options: [option("sim", "Sim"), option("nao", "Não")], dependsOn: { fieldId: "tipo", values: ["eletrico"] } }),
        field("defeitos", "defects", "Peças com defeito", { required: false }),
        field("descricao", "textArea", "Descreva o que aconteceu"),
      ],
    },
    resolutionForm: {
      fields: [
        field("causa", "select", "Causa raiz", { options: [option("desgaste", "Desgaste"), option("operacao", "Erro de operação"), option("projeto", "Falha de projeto")] }),
        field("acao", "textArea", "O que foi feito?"),
      ],
    },
    summaryFields: ["tipo", "descricao"],
    attendantRoleIds: [6, 7],
  });

  sectors.push({
    id: next(),
    name: "Qualidade",
    description: "Defeitos, retrabalho e dúvidas de inspeção.",
    colorHex: "#1E7B3E",
    icon: "quality",
    badges: [],
    active: true,
    externalUrl: null,
    form: {
      fields: [
        field("local", "productionSpot", "Linha e posto"),
        field("defeitos", "defects", "Quantas peças com defeito?", { required: true }),
        field("motivo", "text", "Qual o defeito?"),
      ],
    },
    resolutionForm: { fields: [] },
    summaryFields: ["motivo"],
    attendantRoleIds: [5],
  });

  sectors.push({
    id: next(),
    name: "Sistemas",
    description: "Abre o portal de TI em outra aba.",
    colorHex: "#4B5563",
    icon: "code",
    badges: ["Externo"],
    active: true,
    externalUrl: "https://example.com/ti",
    form: { fields: [] },
    resolutionForm: { fields: [] },
    summaryFields: [],
    attendantRoleIds: [],
  });

  templates.push({ id: next(), name: "Turno do problema", type: "cardRadio", label: "Em qual turno aconteceu?", required: true, options: [option("t1", "Turno 1"), option("t2", "Turno 2")] });

  const sectorOf = (id) => sectors.find((item) => item.id === id);

  const runLookup = (lineId) => productionData.runs.find((run) => run.lineId === lineId && run.status === "running");

  const spotOf = (form, answers) => {
    const spot = form.fields.find((item) => item.type === "productionSpot");
    const stop = form.fields.find((item) => item.type === "lineStop");
    const defect = form.fields.find((item) => item.type === "defects");
    return {
      lineId: spot && answers[spot.id] ? Number(answers[spot.id]) : null,
      postId: spot && answers[`${spot.id}_post`] ? Number(answers[`${spot.id}_post`]) : null,
      stopped: Boolean(stop && answers[stop.id] === "stopped"),
      stoppedAt: stop && answers[`${stop.id}_time`] ? new Date(answers[`${stop.id}_time`]).getTime() : null,
      defects: defect && answers[defect.id] ? Number(answers[defect.id]) : 0,
    };
  };

  const seedTicket = (sector, actor, answers, status, minutesAgo, extra = {}) => {
    const openedAt = now() - minutesAgo * MINUTE;
    const spot = spotOf(sector.form, answers);
    const post = spot.postId ? runLookup(spot.lineId)?.snapshot.posts.find((item) => item.id === spot.postId) : null;
    const ticket = {
      id: next(),
      sectorId: sector.id,
      status,
      openedBy: actor,
      openedAt,
      lineId: spot.lineId,
      postId: spot.postId,
      postName: post?.name ?? null,
      isLineStopped: spot.stopped,
      stoppedAt: spot.stopped ? (spot.stoppedAt ?? openedAt) : null,
      defects: spot.defects,
      formSnapshot: structuredClone(sector.form),
      answers,
      assignedTo: null,
      startedAt: null,
      resolutionSnapshot: { fields: [] },
      resolutionAnswers: {},
      note: null,
      finishedAt: null,
      rejectedBy: null,
      rejectReason: null,
      ...extra,
    };
    tickets.push(ticket);
    return ticket;
  };

  const maintenance = sectors[0];
  const running = productionData.runs.find((run) => run.status === "running");
  const spotLine = running ? String(running.lineId) : "1";
  const spotPost = running ? String(running.snapshot.posts.find((item) => item.name === "Solda B")?.id ?? "") : "";

  seedTicket(maintenance, 2, { local: spotLine, ...(spotPost ? { local_post: spotPost } : {}), parada: "stopped", parada_time: iso(now() - 25 * MINUTE), tipo: "mecanico", componente: "rolamento", defeitos: "6", descricao: "Rolamento da solda B fez barulho e a esteira parou." }, "open", 25);
  seedTicket(maintenance, 3, { local: spotLine, parada: "running", tipo: "eletrico", queda_energia: "nao", descricao: "Sensor de presença falhando de vez em quando." }, "inProgress", 70, { assignedTo: 1, startedAt: now() - 50 * MINUTE });
  seedTicket(
    maintenance,
    3,
    { local: spotLine, parada: "stopped", parada_time: iso(now() - 6 * HOUR), tipo: "mecanico", componente: "correia", descricao: "Correia rompeu." },
    "resolved",
    6 * 60,
    {
      assignedTo: 1,
      startedAt: now() - 5.8 * HOUR,
      finishedAt: now() - 5 * HOUR,
      resolutionSnapshot: structuredClone(maintenance.resolutionForm),
      resolutionAnswers: { causa: "desgaste", acao: "Troca da correia e ajuste de tensão." },
      note: "Peça reserva consumida.",
    },
  );
  seedTicket(sectors[1], 3, { local: spotLine, defeitos: "12", motivo: "Arranhão na carcaça" }, "open", 40);

  const optionLabel = (form, id, value) => form.fields.find((item) => item.id === id)?.options.find((candidate) => candidate.value === value)?.label ?? value;

  const lineName = (id) => productionData.lines.find((line) => line.id === id)?.name ?? "";

  const views = (form, answers, post, only) => {
    const out = [];
    for (const item of visible(form.fields, answers)) {
      if (only && !only.includes(item.id)) continue;
      const value = answers[item.id];
      if (value === undefined) continue;
      let display = value;
      let at = null;
      if (isChoice(item.type)) display = optionLabel(form, item.id, value);
      if (item.type === "productionSpot") display = `${lineName(Number(value))}${post ? ` · ${post}` : ""}`;
      if (item.type === "lineStop") {
        display = value === "stopped" ? "Linha parada" : "Linha rodando";
        at = value === "stopped" ? (answers[`${item.id}_time`] ?? null) : null;
      }
      if (item.type === "defects") display = `${value} peça(s)`;
      out.push({ label: item.label, type: item.type, display, at });
    }
    return out;
  };

  const attends = (sector, actor) => actor !== 3 && sector.attendantRoleIds.length >= 0;
  const manages = (actor) => actor !== 3;

  const sectorSummary = (sector, actor) => ({
    id: sector.id,
    name: sector.name,
    description: sector.description,
    colorHex: sector.colorHex,
    icon: sector.icon,
    badges: sector.badges,
    active: sector.active,
    externalUrl: sector.externalUrl,
    fieldCount: sector.form.fields.length,
    canAttend: attends(sector, actor),
  });

  const sectorDetail = (sector, actor) => ({ ...sector, canAttend: attends(sector, actor), canManage: manages(actor) });

  const abilities = (ticket, sector, actor) => ({
    canStart: ticket.status === "open" && attends(sector, actor),
    canResolve: ticket.status === "inProgress" && ticket.assignedTo === actor,
    canReject: ticket.status === "open" && attends(sector, actor),
  });

  const ticketSummary = (ticket, actor) => {
    const sector = sectorOf(ticket.sectorId);
    return {
      id: ticket.id,
      sectorId: sector.id,
      sectorName: sector.name,
      sectorColor: sector.colorHex,
      sectorIcon: sector.icon,
      status: ticket.status,
      openedByName: nameOf(ticket.openedBy),
      openedAt: iso(ticket.openedAt),
      lineId: ticket.lineId,
      lineName: ticket.lineId ? lineName(ticket.lineId) : null,
      postName: ticket.postName,
      isLineStopped: ticket.isLineStopped,
      stoppedAt: ticket.stoppedAt ? iso(ticket.stoppedAt) : null,
      assignedToName: ticket.assignedTo ? nameOf(ticket.assignedTo) : null,
      startedAt: ticket.startedAt ? iso(ticket.startedAt) : null,
      finishedAt: ticket.finishedAt ? iso(ticket.finishedAt) : null,
      summary: views(ticket.formSnapshot, ticket.answers, ticket.postName, sector.summaryFields),
      ...abilities(ticket, sector, actor),
    };
  };

  const downtimeSeconds = (ticket) => (ticket.isLineStopped && ticket.status !== "rejected" && ticket.stoppedAt ? Math.max(0, Math.round(((ticket.finishedAt ?? now()) - ticket.stoppedAt) / 1000)) : null);

  const ticketDetail = (ticket, actor) => {
    const sector = sectorOf(ticket.sectorId);
    const resolutionForm = ticket.status === "resolved" ? ticket.resolutionSnapshot : sector.resolutionForm;
    return {
      ...ticketSummary(ticket, actor),
      defects: ticket.defects,
      downtimeSeconds: downtimeSeconds(ticket),
      answers: views(ticket.formSnapshot, ticket.answers, ticket.postName),
      resolutionAnswers: views(ticket.resolutionSnapshot, ticket.resolutionAnswers, null),
      note: ticket.note,
      rejectedByName: ticket.rejectedBy ? nameOf(ticket.rejectedBy) : null,
      rejectReason: ticket.rejectReason,
      hasResolutionForm: resolutionForm.fields.length > 0,
      resolutionForm,
    };
  };

  const readSector = (body, exceptId) => {
    const name = String(body?.name ?? "").trim();
    if (!name) return { problem: "Informe o nome." };
    if (name.length > 60) return { problem: "O nome pode ter no máximo 60 caracteres." };
    if (!/^#[0-9a-fA-F]{6}$/.test(body.colorHex ?? "")) return { problem: "Use uma cor no formato #RRGGBB." };
    if (!ICONS.includes(body.icon)) return { problem: "Escolha um dos ícones da lista." };
    const badges = body.badges ?? [];
    if (badges.length > 5 || badges.some((badge) => !String(badge).trim() || String(badge).trim().length > 30)) return { problem: "Use até 5 etiquetas de até 30 caracteres." };
    const externalUrl = String(body.externalUrl ?? "").trim() || null;
    if (externalUrl && !/^https?:\/\//i.test(externalUrl)) return { problem: "Informe um endereço http:// ou https:// válido." };
    const form = externalUrl ? { fields: [] } : { fields: body.form?.fields ?? [] };
    const resolutionForm = externalUrl ? { fields: [] } : { fields: body.resolutionForm?.fields ?? [] };
    const formProblem = checkForm(form, false) ?? checkForm(resolutionForm, true);
    if (formProblem) return { problem: formProblem };
    const summaryFields = externalUrl ? [] : (body.summaryFields ?? []);
    if (summaryFields.length > 3 || new Set(summaryFields).size !== summaryFields.length || summaryFields.some((id) => !form.fields.find((item) => item.id === id) || isSpecial(form.fields.find((item) => item.id === id).type))) {
      return { problem: "O resumo só pode usar campos de pergunta do formulário (sem localização, parada ou defeitos)." };
    }
    if ((body.attendantRoleIds ?? []).some((id) => !ROLES.some((role) => role.id === id))) return { problem: "Um dos papéis de atendimento não existe." };
    if (sectors.some((item) => item.name.toLowerCase() === name.toLowerCase() && item.id !== exceptId)) return { problem: `Já existe um cadastro chamado "${name}".` };

    return {
      value: {
        name,
        description: String(body.description ?? "").trim() || null,
        colorHex: body.colorHex.toUpperCase(),
        icon: body.icon,
        badges: badges.map((badge) => String(badge).trim()),
        active: Boolean(body.active),
        externalUrl,
        form,
        resolutionForm,
        summaryFields,
        attendantRoleIds: externalUrl ? [] : [...new Set(body.attendantRoleIds ?? [])],
      },
    };
  };

  const readTemplate = (body, exceptId) => {
    const name = String(body?.name ?? "").trim();
    if (!name) return { problem: "Informe o nome." };
    if (!TEMPLATE_TYPES.includes(body.type)) return { problem: "Modelo de campo só pode ser texto, área de texto, número, lista suspensa ou cards." };
    const probe = { fields: [{ id: "campo", type: body.type, label: body.label ?? "", required: Boolean(body.required), options: body.options ?? [], dependsOn: null }] };
    const problem = checkForm(probe, false);
    if (problem) return { problem };
    if (templates.some((item) => item.name.toLowerCase() === name.toLowerCase() && item.id !== exceptId)) return { problem: `Já existe um cadastro chamado "${name}".` };
    return { value: { name, type: body.type, label: String(body.label).trim(), required: Boolean(body.required), options: body.options ?? [] } };
  };

  const lossesFor = (lineId, from, to) => {
    const found = tickets.filter((ticket) => ticket.lineId === lineId && ticket.status !== "rejected");
    return {
      downtimes: found
        .filter((ticket) => ticket.isLineStopped && ticket.stoppedAt && ticket.stoppedAt <= to && (!ticket.finishedAt || ticket.finishedAt >= from))
        .map((ticket) => ({ postId: ticket.postId, from: ticket.stoppedAt, to: ticket.finishedAt ?? to })),
      defects: found.filter((ticket) => ticket.defects > 0 && ticket.openedAt >= from && ticket.openedAt <= to).map((ticket) => ({ postId: ticket.postId, at: ticket.openedAt, quantity: ticket.defects })),
    };
  };

  function handle({ req, res, url, segments, actor, json, send, fail }) {
    const [, , , a, b, c] = segments;
    const refuse = (message, status = 400, title = "Falha na operação") => fail(res, status, title, message);
    const denied = () => fail(res, 403, "Acesso negado", "Sua matrícula não tem permissão para esta operação.");

    if (a === "sectors") {
      if (req.method === "GET" && !b) {
        const includeInactive = url.searchParams.get("includeInactive") === "true" && manages(actor);
        const items = sectors.filter((sector) => includeInactive || sector.active).sort((x, y) => x.name.localeCompare(y.name)).map((sector) => sectorSummary(sector, actor));
        return send(res, 200, { items, canManage: manages(actor) });
      }

      if (req.method === "GET" && b) {
        const sector = sectors.find((item) => String(item.id) === b);
        return sector ? send(res, 200, sectorDetail(sector, actor)) : refuse("Setor não encontrado.", 404, "Falha ao buscar setor");
      }

      if (!manages(actor)) return denied();

      if (req.method === "POST" && !b) {
        const read = readSector(json);
        if (read.problem) return refuse(read.problem, 400, "Falha ao criar setor");
        const created = { id: next(), ...read.value };
        sectors.push(created);
        return send(res, 201, sectorDetail(created, actor));
      }

      if (req.method === "PUT" && b) {
        const sector = sectors.find((item) => String(item.id) === b);
        if (!sector) return refuse("Setor não encontrado.", 404);
        const read = readSector(json, sector.id);
        if (read.problem) return refuse(read.problem, 400, "Falha ao salvar setor");
        Object.assign(sector, read.value);
        return send(res, 204);
      }
    }

    if (a === "field-templates") {
      if (req.method === "GET") return send(res, 200, [...templates].sort((x, y) => x.name.localeCompare(y.name)));
      if (!manages(actor)) return denied();

      if (req.method === "POST") {
        const read = readTemplate(json);
        if (read.problem) return refuse(read.problem, 400, "Falha ao criar modelo de campo");
        const created = { id: next(), ...read.value };
        templates.push(created);
        return send(res, 201, created);
      }

      const template = templates.find((item) => String(item.id) === b);
      if (!template) return refuse("Modelo de campo não encontrado.", 404);

      if (req.method === "PUT") {
        const read = readTemplate(json, template.id);
        if (read.problem) return refuse(read.problem, 400);
        Object.assign(template, read.value);
        return send(res, 204);
      }

      if (req.method === "DELETE") {
        templates.splice(templates.indexOf(template), 1);
        return send(res, 204);
      }
    }

    if (a === "roles" && req.method === "GET") return manages(actor) ? send(res, 200, ROLES) : denied();

    if (a === "locations" && req.method === "GET") {
      return send(
        res,
        200,
        productionData.lines
          .filter((line) => line.active)
          .map((line) => {
            const run = runLookup(line.id);
            return { lineId: line.id, lineName: line.name, posts: run ? [...run.snapshot.posts].sort((x, y) => x.x - y.x).map((post) => ({ id: post.id, name: post.name })) : [] };
          }),
      );
    }

    if (a === "tickets") {
      if (req.method === "GET" && !b) {
        const scope = url.searchParams.get("scope") === "mine" ? "mine" : "attend";
        const status = url.searchParams.get("status");
        const sectorId = Number(url.searchParams.get("sectorId") ?? 0);
        const lineId = Number(url.searchParams.get("lineId") ?? 0);
        const from = url.searchParams.get("from") ? new Date(`${url.searchParams.get("from")}T00:00:00.000Z`).getTime() : null;
        const to = url.searchParams.get("to") ? new Date(`${url.searchParams.get("to")}T23:59:59.999Z`).getTime() : null;
        if (from !== null && to !== null && to < from) return refuse("O fim do período não pode ser anterior ao início.");
        const page = Math.max(0, Number(url.searchParams.get("page") ?? 0));
        const pageSize = Math.min(100, Math.max(1, Number(url.searchParams.get("pageSize") ?? 20)));
        const canAttend = sectors.some((sector) => attends(sector, actor));

        const base = tickets
          .filter((ticket) => (scope === "mine" ? ticket.openedBy === actor : attends(sectorOf(ticket.sectorId), actor)))
          .filter((ticket) => !sectorId || ticket.sectorId === sectorId)
          .filter((ticket) => !lineId || ticket.lineId === lineId)
          .filter((ticket) => from === null || ticket.openedAt >= from)
          .filter((ticket) => to === null || ticket.openedAt <= to);

        const filtered = base.filter((ticket) => !status || ticket.status === status).sort((x, y) => y.openedAt - x.openedAt);
        const count = (value) => base.filter((ticket) => ticket.status === value).length;

        return send(res, 200, {
          items: filtered.slice(page * pageSize, (page + 1) * pageSize).map((ticket) => ticketSummary(ticket, actor)),
          totalCount: filtered.length,
          page,
          pageSize,
          counts: { open: count("open"), inProgress: count("inProgress"), resolved: count("resolved"), rejected: count("rejected") },
          canAttend: scope === "mine" ? canAttend : canAttend,
        });
      }

      if (req.method === "POST" && !b) {
        const sector = sectorOf(json?.sectorId);
        if (!sector) return refuse("Setor não encontrado.", 404, "Falha ao abrir chamado");
        if (!sector.active) return refuse("O setor está desativado e não recebe chamados.");
        if (sector.externalUrl) return refuse("Este setor abre um endereço externo e não recebe chamados aqui.");
        if (sector.form.fields.length === 0) return refuse("Este setor ainda não tem formulário.");
        const normalized = normalize(sector.form, json.answers, now());
        if (normalized.problem) return refuse(normalized.problem, 400, "Falha ao abrir chamado");
        const spot = spotOf(sector.form, normalized.answers);
        let postName = null;

        if (spot.lineId) {
          const line = productionData.lines.find((item) => item.id === spot.lineId && item.active);
          if (!line) return refuse("A linha selecionada não existe ou está desativada.");
          if (spot.postId) {
            postName = runLookup(spot.lineId)?.snapshot.posts.find((item) => item.id === spot.postId)?.name ?? null;
            if (!postName) return refuse("O posto escolhido não faz parte da produção em andamento nessa linha.");
          }
        } else if (spot.stopped) {
          return refuse("Selecione a linha.");
        }

        const ticket = seedTicket(sector, actor, normalized.answers, "open", 0);
        ticket.postName = postName;
        return send(res, 201, ticketSummary(ticket, actor));
      }

      const ticket = tickets.find((item) => String(item.id) === b);
      if (!ticket) return refuse("Chamado não encontrado.", 404, "Falha ao buscar chamado");
      const sector = sectorOf(ticket.sectorId);

      if (req.method === "GET" && !c) {
        if (ticket.openedBy !== actor && !attends(sector, actor)) return fail(res, 403, "Acesso negado", "Seu perfil não atende chamados deste setor.");
        return send(res, 200, ticketDetail(ticket, actor));
      }

      if (req.method === "POST" && c === "start") {
        if (!attends(sector, actor)) return fail(res, 403, "Acesso negado", "Seu perfil não atende chamados deste setor.");
        if (ticket.status !== "open") return refuse("Este chamado já foi assumido, recusado ou finalizado.");
        Object.assign(ticket, { status: "inProgress", assignedTo: actor, startedAt: now() });
        return send(res, 204);
      }

      if (req.method === "POST" && c === "resolve") {
        if (ticket.status !== "inProgress") return refuse("Só dá para finalizar um chamado que está em atendimento.");
        if (ticket.assignedTo !== actor) return refuse("Só quem assumiu o chamado pode finalizá-lo.");
        let answers = {};
        if (sector.resolutionForm.fields.length > 0) {
          const normalized = normalize(sector.resolutionForm, json?.answers, now());
          if (normalized.problem) return refuse(normalized.problem, 400, "Falha ao finalizar chamado");
          answers = normalized.answers;
        }
        Object.assign(ticket, { status: "resolved", resolutionSnapshot: structuredClone(sector.resolutionForm), resolutionAnswers: answers, note: String(json?.note ?? "").trim() || null, finishedAt: now() });
        return send(res, 204);
      }

      if (req.method === "POST" && c === "reject") {
        if (!attends(sector, actor)) return fail(res, 403, "Acesso negado", "Seu perfil não atende chamados deste setor.");
        if (ticket.status !== "open") return refuse("Este chamado já foi assumido, recusado ou finalizado.");
        const reason = String(json?.reason ?? "").trim();
        if (!reason) return refuse("Informe o motivo da recusa.");
        Object.assign(ticket, { status: "rejected", rejectedBy: actor, rejectReason: reason, finishedAt: now() });
        return send(res, 204);
      }
    }

    return fail(res, 404, "Rota inexistente", url.pathname);
  }

  handle.data = { tickets, lossesFor };
  return handle;
}
