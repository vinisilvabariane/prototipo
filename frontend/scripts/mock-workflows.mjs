import { parseMultipart } from "./mock-multipart.mjs";

const MAX_ATTACHMENTS = 5;
const MAX_BYTES = 10 * 1024 * 1024;

const now = (offsetMinutes = 0) => new Date(Date.now() + offsetMinutes * 60_000).toISOString();

let counter = 1000;
const nextId = () => ++counter;

const workflows = [];

function addStep(workflow, userId, action, description, justification, review = "none", at = now()) {
  const step = { id: nextId(), action, description, justification: justification ?? null, userId, createdAt: at, review };
  workflow.steps.push(step);
  return step;
}

const isCreator = (workflow, userId) => workflow.creatorId === userId;
const isAssignee = (workflow, userId) => workflow.assignees.some((assignee) => assignee.userId === userId);
const hasApproved = (workflow, userId) => workflow.steps.some((step) => step.action === "approval" && step.userId === userId);
const everyoneApproved = (workflow) => workflow.assignees.every((assignee) => hasApproved(workflow, assignee.userId));
const pendingRejection = (workflow, userId) =>
  [...workflow.steps].reverse().find((step) => step.action === "rejection" && step.userId === userId && step.review !== "none");
const awaitingCreator = (workflow) => workflow.steps.some((step) => step.action === "rejection" && step.review === "creatorReview");

const invalid = (message) => ({ kind: 400, message });
const forbidden = (message) => ({ kind: 403, message });
const missing = (message) => ({ kind: 404, message });
const finished = (workflow) =>
  workflow.status === "inProgress" ? null : invalid(`Este workflow já foi ${workflow.status === "approved" ? "aprovado" : "rejeitado"} e não aceita novas ações.`);

const checks = {
  edit: (w, actor) => (isCreator(w, actor) ? finished(w) : forbidden("Somente o criador do workflow pode editá-lo.")),
  approve: (w, actor) => {
    if (!isAssignee(w, actor)) return forbidden("Você não é responsável por este workflow.");
    if (finished(w)) return finished(w);
    if (hasApproved(w, actor)) return invalid("Você já aprovou este workflow.");
    return pendingRejection(w, actor)?.review === "creatorReview" ? invalid("Sua rejeição está em análise pelo criador do workflow.") : null;
  },
  reject: (w, actor) => {
    if (!isAssignee(w, actor)) return forbidden("Você não é responsável por este workflow.");
    if (finished(w)) return finished(w);
    if (hasApproved(w, actor)) return invalid("Você já aprovou este workflow.");
    return pendingRejection(w, actor) ? invalid("Você já tem uma rejeição em andamento neste workflow.") : null;
  },
  accept: (w, actor) => {
    if (!isCreator(w, actor)) return forbidden("Somente o criador do workflow pode analisar uma rejeição.");
    if (finished(w)) return finished(w);
    return awaitingCreator(w) ? null : invalid("Não há nenhuma rejeição aguardando a análise do criador.");
  },
  returnToAssignee: (w, actor, stepId) => {
    if (!isCreator(w, actor)) return forbidden("Somente o criador do workflow pode analisar uma rejeição.");
    if (finished(w)) return finished(w);
    const step = w.steps.find((candidate) => candidate.id === stepId);
    if (!step) return missing("Etapa do workflow não encontrada.");
    return step.action === "rejection" && step.review === "creatorReview" ? null : invalid("Esta rejeição não está aguardando a análise do criador.");
  },
  returnToCreator: (w, actor, stepId) => {
    const step = w.steps.find((candidate) => candidate.id === stepId);
    if (!step) return missing("Etapa do workflow não encontrada.");
    if (step.action !== "rejection") return invalid("A etapa informada não é uma rejeição.");
    if (step.userId !== actor) return forbidden("Somente quem rejeitou o workflow pode revisar esta rejeição.");
    if (finished(w)) return finished(w);
    return step.review === "assigneeReview" ? null : invalid("Esta rejeição não está aguardando a revisão de quem a fez.");
  },
  addAssignee: (w, actor, userId) => {
    if (!isCreator(w, actor)) return forbidden("Somente o criador do workflow pode alterar os responsáveis.");
    if (finished(w)) return finished(w);
    if (isCreator(w, userId)) return invalid("O criador do workflow já é automaticamente um responsável.");
    return isAssignee(w, userId) ? invalid("Este usuário já é responsável por este workflow.") : null;
  },
  replaceAssignee: (w, actor, oldId, newId) => {
    if (!isCreator(w, actor)) return forbidden("Somente o criador do workflow pode alterar os responsáveis.");
    if (finished(w)) return finished(w);
    if (!isAssignee(w, oldId)) return missing("Responsável não encontrado para este workflow.");
    if (isCreator(w, oldId)) return invalid("Não é possível remover o criador do workflow.");
    if (hasApproved(w, oldId)) return invalid("Não é possível substituir um responsável que já aprovou o workflow.");
    return isAssignee(w, newId) ? invalid("O novo responsável selecionado já está na lista de responsáveis.") : null;
  },
};

function completeIfApproved(workflow, actor, description, note) {
  if (workflow.status !== "inProgress" || !everyoneApproved(workflow)) return false;
  workflow.status = "approved";
  workflow.completedAt = now();
  addStep(workflow, actor, "finalApproval", description, note);
  return true;
}

function createWorkflow({ title, description, priority, category, dueDate, creatorId, assigneeIds }) {
  const at = now();
  const workflow = {
    id: nextId(),
    title: title.trim(),
    description: (description ?? "").trim(),
    priority,
    category: category || null,
    status: "inProgress",
    creatorId,
    createdAt: at,
    completedAt: null,
    dueDate: dueDate || null,
    assignees: [...new Set([...assigneeIds, creatorId])].map((userId) => ({ userId, assignedAt: at })),
    steps: [],
    attachments: [],
    messages: [],
  };
  addStep(workflow, creatorId, "created", "Workflow criado pelo usuário", "Workflow criado", "none", at);
  addStep(workflow, creatorId, "approval", "Aprovação automática do criador", "Aprovado automaticamente ao criar o workflow", "none", at);
  completeIfApproved(workflow, creatorId, "Workflow aprovado por todos os responsáveis", "Aprovação final concluída");
  workflows.push(workflow);
  return workflow;
}

function seed(users) {
  const [ana, bruno, carla] = users.map((user) => user.id);
  const inFuture = (days) => new Date(Date.now() + days * 86400_000).toISOString().slice(0, 10);

  const plan = createWorkflow({
    title: "Revisão do plano de controle da linha 3",
    description: "Revisar o plano de controle após a mudança de fornecedor da carcaça.\nConferir características especiais e frequência de inspeção.",
    priority: "high",
    category: "procedures",
    dueDate: inFuture(5),
    creatorId: ana,
    assigneeIds: [bruno, carla],
  });

  const norm = createWorkflow({
    title: "Atualização da norma de rastreabilidade",
    description: "Adequar o procedimento de rastreabilidade à nova revisão da norma.",
    priority: "medium",
    category: "standards",
    dueDate: inFuture(-2),
    creatorId: bruno,
    assigneeIds: [ana],
  });
  addStep(norm, ana, "rejection", "Workflow rejeitado pelo usuário", "A seção 4.2 ainda cita a revisão antiga da norma.", "creatorReview");

  const done = createWorkflow({
    title: "Requisitos do cliente Alfa — embalagem",
    description: "",
    priority: "low",
    category: "requirements",
    dueDate: inFuture(10),
    creatorId: carla,
    assigneeIds: [],
  });
  void plan;
  void done;
}

const summary = (w) => ({
  id: w.id,
  title: w.title,
  description: w.description,
  priority: w.priority,
  category: w.category,
  status: w.status,
  createdAt: w.createdAt,
  dueDate: w.dueDate,
  completedAt: w.completedAt,
});

function reviewRole(workflow, step, actor) {
  if (workflow.status !== "inProgress" || step.action !== "rejection") return "none";
  if (step.review === "creatorReview" && isCreator(workflow, actor)) return "creator";
  return step.review === "assigneeReview" && step.userId === actor ? "assignee" : "none";
}

export function createWorkflowMock(users) {
  seed(users);

  const person = (id) => users.find((user) => user.id === id);
  const nameOf = (id) => person(id)?.name ?? "Usuário não identificado";

  const detail = (w, actor) => ({
    ...summary(w),
    creatorId: w.creatorId,
    creatorName: nameOf(w.creatorId),
    assignees: [...w.assignees]
      .sort((a, b) => b.assignedAt.localeCompare(a.assignedAt))
      .map((assignee) => ({
        userId: assignee.userId,
        name: nameOf(assignee.userId),
        registration: person(assignee.userId)?.registration ?? 0,
        assignedAt: assignee.assignedAt,
        approved: hasApproved(w, assignee.userId),
      })),
    attachments: [...w.attachments].reverse().map(({ data, ...rest }) => rest),
    steps: [...w.steps].reverse().map((step) => ({
      ...step,
      userName: nameOf(step.userId),
      actorRole: reviewRole(w, step, actor),
    })),
    permissions: {
      isCreator: isCreator(w, actor),
      isAssignee: isAssignee(w, actor),
      canApprove: !checks.approve(w, actor),
      canReject: !checks.reject(w, actor),
      canEdit: !checks.edit(w, actor),
      canManageAssignees: isCreator(w, actor) && w.status === "inProgress",
    },
  });

  return function handle({ req, res, url, segments, actor, json, raw, contentType, send, fail }) {
    const [, , , id, sub, subId, leaf] = segments;
    const refuse = (refusal) => fail(res, refusal.kind, refusal.kind === 403 ? "Acesso negado" : refusal.kind === 404 ? "Não encontrado" : "Falha na operação", refusal.message);
    const need = () => {
      const w = workflows.find((candidate) => String(candidate.id) === id);
      if (!w) fail(res, 404, "Falha ao buscar workflow", "Workflow não encontrado.");
      return w;
    };
    const reason = () => {
      const text = String(json.justification ?? "").trim();
      if (!text) {
        fail(res, 400, "Requisição inválida", "Informe uma justificativa.");
        return null;
      }
      return text;
    };

    if (req.method === "GET" && !id) {
      const page = Number(url.searchParams.get("page") ?? 0);
      const size = Number(url.searchParams.get("pageSize") ?? 10);
      const search = (url.searchParams.get("search") ?? "").trim().toLowerCase();
      const filter = (url.searchParams.get("filter") ?? "all").toLowerCase();
      const found = workflows
        .filter((w) => filter === "mine" ? isCreator(w, actor) : filter === "participating" ? isAssignee(w, actor) : true)
        .filter((w) => !search || w.title.toLowerCase().includes(search) || w.description.toLowerCase().includes(search))
        .sort((a, b) => b.createdAt.localeCompare(a.createdAt) || b.id - a.id);
      return send(res, 200, { items: found.slice(page * size, page * size + size).map(summary), totalCount: found.length, page, pageSize: size });
    }

    if (req.method === "GET" && id === "stats") {
      const count = (status) => workflows.filter((w) => w.status === status).length;
      return send(res, 200, { total: workflows.length, inProgress: count("inProgress"), approved: count("approved"), rejected: count("rejected") });
    }

    if (req.method === "GET" && id === "users") {
      return send(res, 200, [...users].sort((a, b) => a.name.localeCompare(b.name)).map(({ id: userId, registration, name }) => ({ id: userId, registration, name })));
    }

    if (req.method === "POST" && !id) {
      const fields = parseMultipart(raw, contentType);
      const value = (name) => fields.find((field) => field.name === name && !field.filename)?.content.toString() ?? "";
      const files = fields.filter((field) => field.name === "attachments" && field.filename);

      if (!value("title").trim()) return fail(res, 400, "Requisição inválida", "Corpo da requisição inválido ou mal formatado.");
      if (!["high", "medium", "low"].includes(value("priority"))) return fail(res, 400, "Falha ao criar workflow", "Selecione uma prioridade válida.");
      if (!value("dueDate")) return fail(res, 400, "Falha ao criar workflow", "Informe o prazo do workflow.");
      if (files.length > MAX_ATTACHMENTS) return fail(res, 400, "Falha ao criar workflow", "Você pode enviar no máximo 5 arquivos.");
      const heavy = files.find((file) => file.content.length > MAX_BYTES);
      if (heavy) return fail(res, 400, "Falha ao criar workflow", `O arquivo "${heavy.filename}" excede o tamanho máximo de 10MB.`);

      const assigneeIds = fields.filter((field) => field.name === "assigneeIds").map((field) => Number(field.content.toString()));
      if (assigneeIds.some((userId) => !person(userId))) {
        return fail(res, 400, "Falha ao criar workflow", "Um ou mais responsáveis informados não existem ou estão desativados.");
      }

      const workflow = createWorkflow({
        title: value("title"),
        description: value("description"),
        priority: value("priority"),
        category: value("category"),
        dueDate: value("dueDate"),
        creatorId: actor,
        assigneeIds,
      });

      for (const file of files) {
        workflow.attachments.push({
          id: nextId(),
          name: file.filename,
          contentType: file.type ?? "application/octet-stream",
          size: file.content.length,
          uploadedAt: now(),
          data: file.content,
        });
      }

      return send(res, 201, summary(workflow));
    }

    if (!id) return fail(res, 404, "Rota inexistente", url.pathname);

    const workflow = need();
    if (!workflow) return;

    if (req.method === "GET" && !sub) return send(res, 200, detail(workflow, actor));

    if (req.method === "PUT" && !sub) {
      const refusal = checks.edit(workflow, actor);
      if (refusal) return refuse(refusal);
      if (!String(json.title ?? "").trim()) return fail(res, 400, "Requisição inválida", "Informe o título do workflow.");

      const next = { title: json.title.trim(), description: String(json.description ?? "").trim(), priority: json.priority, category: json.category || null };
      const changed = Object.keys(next).some((key) => workflow[key] !== next[key]);
      if (changed) {
        Object.assign(workflow, next);
        addStep(workflow, actor, "edited", "Workflow editado", "Processo editado pelo criador do workflow");
      }
      return send(res, 204);
    }

    if (req.method === "POST" && sub === "approve") {
      const refusal = checks.approve(workflow, actor);
      if (refusal) return refuse(refusal);
      const text = reason();
      if (text === null) return;

      const step = pendingRejection(workflow, actor);
      if (step) step.review = "none";
      addStep(workflow, actor, "approval", "Workflow aprovado pelo usuário", text);
      const done = completeIfApproved(workflow, actor, "Workflow aprovado por todos os responsáveis", "Aprovação final concluída");
      return send(res, 200, {
        status: workflow.status,
        message: done ? "Workflow aprovado por todos os responsáveis." : "Sua aprovação foi registrada. Aguardando os demais responsáveis.",
      });
    }

    if (req.method === "POST" && sub === "reject") {
      const refusal = checks.reject(workflow, actor);
      if (refusal) return refuse(refusal);
      const text = reason();
      if (text === null) return;

      addStep(workflow, actor, "rejection", "Workflow rejeitado pelo usuário", text, "creatorReview");
      return send(res, 200, { status: workflow.status, message: "Sua rejeição foi registrada e aguarda a análise do criador." });
    }

    if (req.method === "POST" && sub === "accept-rejection") {
      const refusal = checks.accept(workflow, actor);
      if (refusal) return refuse(refusal);
      const text = reason();
      if (text === null) return;

      for (const step of workflow.steps) step.review = "none";
      workflow.status = "rejected";
      workflow.completedAt = now();
      addStep(workflow, actor, "finalRejection", "Workflow rejeitado pelo criador", text);
      return send(res, 200, { status: workflow.status, message: "Workflow rejeitado com sucesso." });
    }

    if (req.method === "POST" && sub === "steps" && (leaf === "return-to-assignee" || leaf === "return-to-creator")) {
      const stepId = Number(subId);
      const toAssignee = leaf === "return-to-assignee";
      const refusal = toAssignee ? checks.returnToAssignee(workflow, actor, stepId) : checks.returnToCreator(workflow, actor, stepId);
      if (refusal) return refuse(refusal);
      const text = reason();
      if (text === null) return;

      workflow.steps.find((step) => step.id === stepId).review = toAssignee ? "assigneeReview" : "creatorReview";
      workflow.messages.push({ id: nextId(), stepId, userId: actor, text, createdAt: now() });
      return send(res, 204);
    }

    if (req.method === "GET" && sub === "steps" && leaf === "messages") {
      const stepId = Number(subId);
      if (!workflow.steps.some((step) => step.id === stepId)) return fail(res, 404, "Falha ao carregar as mensagens", "Etapa do workflow não encontrada.");
      return send(
        res,
        200,
        workflow.messages
          .filter((message) => message.stepId === stepId)
          .map((message) => ({ id: message.id, userId: message.userId, userName: nameOf(message.userId), text: message.text, createdAt: message.createdAt })),
      );
    }

    if (req.method === "POST" && sub === "assignees" && !subId) {
      const userId = Number(json.userId);
      const refusal = checks.addAssignee(workflow, actor, userId);
      if (refusal) return refuse(refusal);
      const user = person(userId);
      if (!user) return fail(res, 404, "Falha ao adicionar responsável", "Usuário não encontrado.");

      const at = now();
      workflow.assignees.push({ userId, assignedAt: at });
      addStep(workflow, actor, "assigneeAdded", `Responsável adicionado: ${user.name} (${user.registration})`, null);
      return send(res, 201, { userId, name: user.name, registration: user.registration, assignedAt: at, approved: false });
    }

    if (req.method === "POST" && sub === "assignees" && leaf === "replace") {
      const oldId = Number(subId);
      const newId = Number(json.newUserId);
      const refusal = checks.replaceAssignee(workflow, actor, oldId, newId);
      if (refusal) return refuse(refusal);
      const user = person(newId);
      if (!user) return fail(res, 404, "Falha ao substituir responsável", "O novo responsável não existe ou está desativado.");

      workflow.assignees = workflow.assignees.filter((assignee) => assignee.userId !== oldId);
      workflow.assignees.push({ userId: newId, assignedAt: now() });
      for (const step of workflow.steps.filter((candidate) => candidate.userId === oldId)) step.review = "none";
      const previous = person(oldId);
      addStep(
        workflow,
        actor,
        "assigneeReplaced",
        `Substituição de responsável: ${previous.name} (${previous.registration}) → ${user.name} (${user.registration})`,
        "Responsável substituído mediante solicitação do criador",
      );
      const done = completeIfApproved(workflow, actor, "Workflow aprovado por todos os responsáveis após substituição", "Aprovação final concluída automaticamente após substituição de responsável");
      return send(res, 200, { status: workflow.status, message: done ? "Responsável substituído e workflow aprovado automaticamente." : "Responsável substituído com sucesso." });
    }

    if (req.method === "GET" && sub === "attachments") {
      const attachment = workflow.attachments.find((candidate) => String(candidate.id) === subId);
      if (!attachment) return fail(res, 404, "Falha ao baixar anexo", "Anexo não encontrado.");
      res.writeHead(200, {
        "Content-Type": attachment.contentType,
        "Content-Disposition": `attachment; filename="${attachment.name}"`,
        "Content-Length": attachment.data.length,
      });
      return res.end(attachment.data);
    }

    return fail(res, 404, "Rota inexistente", url.pathname);
  };
}
