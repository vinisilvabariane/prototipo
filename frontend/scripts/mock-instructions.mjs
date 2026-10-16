import { parseMultipart } from "./mock-multipart.mjs";

const MAX_STEPS = 50;
const MAX_PDF = 10 * 1024 * 1024;
const MAX_IMAGE = 5 * 1024 * 1024;

const isPdf = (buffer) => buffer.subarray(0, 5).toString("latin1") === "%PDF-";

function imageType(buffer) {
  if (buffer.subarray(0, 8).equals(Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]))) return "image/png";
  if (buffer[0] === 0xff && buffer[1] === 0xd8 && buffer[2] === 0xff) return "image/jpeg";
  const head = buffer.subarray(0, 6).toString("latin1");
  if (head === "GIF87a" || head === "GIF89a") return "image/gif";
  if (buffer.subarray(0, 4).toString("latin1") === "RIFF" && buffer.subarray(8, 12).toString("latin1") === "WEBP") return "image/webp";
  return null;
}

const REQUIRED = [
  ["code", "Informe o código da instrução."],
  ["title", "Informe o título da instrução."],
  ["documentNumber", "Informe o número do documento."],
  ["product", "Informe o produto."],
  ["preparedBy", "Informe quem elaborou a instrução."],
  ["approvedBy", "Informe quem aprovou a instrução."],
  ["requiredPpe", "Informe os EPIs obrigatórios."],
  ["processSpecifications", "Informe as especificações de processo."],
];

export function createInstructionMock(users) {
  let counter = 700;
  const next = () => ++counter;
  const instructions = [];
  const now = () => new Date().toISOString();
  const nameOf = (id) => users.find((user) => user.id === id)?.name ?? "Usuário não identificado";

  const samplePdf = (title) =>
    Buffer.from(
      `%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 300 144]/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>>endobj\n4 0 obj<</Length 60>>stream\nBT /F1 14 Tf 20 70 Td (${title}) Tj ET\nendstream endobj\n5 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF`,
      "latin1",
    );

  const seed = (code, title, product, steps) => {
    const at = now();
    const pdf = samplePdf(title);
    const instruction = {
      id: next(),
      code,
      title,
      documentNumber: "DOC-" + code.slice(-3),
      product,
      preparedBy: "Ana Souza",
      approvedBy: "Bruno Lima",
      requiredPpe: "Luvas e óculos de proteção",
      processSpecifications: "Temperatura de 350 °C, tempo de 4 s por ponto.",
      notes: null,
      pdf,
      createdBy: users[0].id,
      createdAt: at,
      updatedBy: users[0].id,
      updatedAt: at,
      steps: steps.map((text, index) => ({ id: next(), position: index + 1, text, image: null })),
    };
    instructions.push(instruction);
    return instruction;
  };

  seed("IT-20261001-ABC12", "Soldagem da carcaça", "Fone X", ["Posicionar a carcaça no gabarito.", "Acionar o ferro de solda e aplicar nos quatro pontos.", "Conferir a solda visualmente."]);
  seed("IT-20261002-DEF34", "Embalagem final do produto", "Caixa de som Y", ["Inserir o produto no blister.", "Fechar a caixa e aplicar a etiqueta."]);

  const summary = (item) => ({
    id: item.id,
    code: item.code,
    title: item.title,
    documentNumber: item.documentNumber,
    product: item.product,
    preparedBy: item.preparedBy,
    approvedBy: item.approvedBy,
    stepCount: item.steps.length,
    pdfSize: item.pdf.length,
    createdAt: item.createdAt,
    updatedAt: item.updatedAt,
  });

  const detail = (item, canManage) => ({
    id: item.id,
    code: item.code,
    title: item.title,
    documentNumber: item.documentNumber,
    product: item.product,
    preparedBy: item.preparedBy,
    approvedBy: item.approvedBy,
    requiredPpe: item.requiredPpe,
    processSpecifications: item.processSpecifications,
    notes: item.notes,
    pdfSize: item.pdf.length,
    createdBy: nameOf(item.createdBy),
    createdAt: item.createdAt,
    updatedBy: nameOf(item.updatedBy),
    updatedAt: item.updatedAt,
    steps: item.steps.map((step) => ({ id: step.id, position: step.position, text: step.text, hasImage: Boolean(step.image) })),
    canManage,
  });

  const stepFields = (fields) => {
    const drafts = new Map();
    for (const field of fields) {
      const match = /^steps\[(\d+)\]\.(text|id|removeImage)$/.exec(field.name ?? "");
      if (!match || field.filename) continue;
      const draft = drafts.get(Number(match[1])) ?? {};
      draft[match[2]] = field.content.toString();
      drafts.set(Number(match[1]), draft);
    }
    return [...drafts.entries()].sort((a, b) => a[0] - b[0]).map(([index, draft]) => ({ index, text: draft.text ?? "", id: draft.id ? Number(draft.id) : null, removeImage: draft.removeImage === "true" }));
  };

  return function handle({ req, res, url, segments, actor, raw, contentType, send, fail }) {
    const [, , , a, b, stepId, leaf] = segments;
    const canManage = actor !== 3;
    const denied = () => fail(res, 403, "Acesso negado", "Sua matrícula não tem permissão para esta operação.");
    const failure = (details, status = 400) => fail(res, status, "Falha ao salvar instrução de trabalho", details);

    if (req.method === "GET" && !a) {
      const page = Number(url.searchParams.get("page") ?? 0);
      const size = Number(url.searchParams.get("pageSize") ?? 10);
      const search = (url.searchParams.get("search") ?? "").trim().toLowerCase();
      const found = instructions
        .filter((item) => !search || [item.title, item.code, item.product, item.documentNumber].some((value) => value.toLowerCase().includes(search)))
        .sort((x, y) => x.title.localeCompare(y.title));
      return send(res, 200, { items: found.slice(page * size, page * size + size).map(summary), totalCount: found.length, page, pageSize: size, canManage });
    }

    if (req.method === "GET" && a === "next-code") {
      if (!canManage) return denied();
      const alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
      const suffix = Array.from({ length: 5 }, () => alphabet[Math.floor(Math.random() * alphabet.length)]).join("");
      return send(res, 200, { code: `IT-${new Date().toISOString().slice(0, 10).replaceAll("-", "")}-${suffix}` });
    }

    if (req.method === "POST" && !a) {
      if (!canManage) return denied();
      return save(null);
    }

    const item = instructions.find((candidate) => String(candidate.id) === a);
    if (!item) return fail(res, 404, "Falha ao buscar instrução de trabalho", "Instrução de trabalho não encontrada.");

    if (req.method === "GET" && !b) return send(res, 200, detail(item, canManage));

    if (req.method === "GET" && b === "pdf") {
      res.writeHead(200, { "Content-Type": "application/pdf", "Content-Disposition": `inline; filename*=UTF-8''${item.code}.pdf`, "Content-Length": item.pdf.length });
      return res.end(item.pdf);
    }

    if (req.method === "GET" && b === "steps" && leaf === "image") {
      const step = item.steps.find((candidate) => String(candidate.id) === stepId);
      if (!step?.image) return fail(res, 404, "Falha ao abrir a imagem", "Arquivo não encontrado.");
      res.writeHead(200, { "Content-Type": step.image.type, "Content-Length": step.image.data.length });
      return res.end(step.image.data);
    }

    if (!canManage) return denied();

    if (req.method === "DELETE" && !b) {
      instructions.splice(instructions.indexOf(item), 1);
      return send(res, 204);
    }

    if (req.method === "PUT" && !b) return save(item);

    return fail(res, 404, "Rota inexistente", url.pathname);

    function save(existing) {
      const fields = parseMultipart(raw, contentType);
      const value = (name) => fields.find((field) => field.name === name && !field.filename)?.content.toString().trim() ?? "";

      for (const [key, message] of REQUIRED) if (!value(key)) return failure(message);

      const code = value("code").toUpperCase();
      if (instructions.some((other) => other.code === code && other !== existing)) return failure(`O código "${code}" já está em uso.`);

      const drafts = stepFields(fields);
      if (drafts.length > MAX_STEPS) return failure(`A instrução pode ter no máximo ${MAX_STEPS} passos.`);
      if (drafts.some((draft) => !draft.text.trim())) return failure("Todo passo precisa de uma instrução escrita.");

      const pdf = fields.find((field) => field.name === "pdf" && field.filename);
      if (!existing && !pdf) return failure("Anexe o PDF da instrução.");
      if (pdf && pdf.content.length > MAX_PDF) return failure("O PDF passa do tamanho máximo de 10 MB.");
      if (pdf && !isPdf(pdf.content)) return failure("O arquivo enviado não é um PDF válido.");

      const images = new Map();
      for (const draft of drafts) {
        const file = fields.find((field) => field.name === `stepImages[${draft.index}]` && field.filename);
        if (!file) continue;
        if (file.content.length > MAX_IMAGE) return failure("A imagem do passo passa do tamanho máximo de 5 MB.");
        const type = imageType(file.content);
        if (!type) return failure("A imagem do passo precisa ser PNG, JPEG, GIF ou WebP.");
        images.set(draft.index, { type, data: file.content });
      }

      const target = existing ?? { id: next(), createdBy: actor, createdAt: now(), steps: [] };

      const steps = [];
      for (const draft of drafts) {
        const kept = draft.id !== null ? target.steps.find((step) => step.id === draft.id) : null;
        if (draft.id !== null && !kept) return fail(res, 404, "Falha ao salvar instrução de trabalho", "Passo não encontrado.");

        const image = images.get(draft.index) ?? (draft.removeImage ? null : (kept?.image ?? null));
        steps.push({ id: kept?.id ?? next(), position: steps.length + 1, text: draft.text.trim(), image });
      }

      Object.assign(target, {
        code,
        title: value("title"),
        documentNumber: value("documentNumber"),
        product: value("product"),
        preparedBy: value("preparedBy"),
        approvedBy: value("approvedBy"),
        requiredPpe: value("requiredPpe"),
        processSpecifications: value("processSpecifications"),
        notes: value("notes") || null,
        pdf: pdf ? pdf.content : (target.pdf ?? Buffer.alloc(0)),
        updatedBy: actor,
        updatedAt: now(),
        steps,
      });

      if (!existing) instructions.push(target);
      return existing ? send(res, 204) : send(res, 201, summary(target));
    }
  };
}
