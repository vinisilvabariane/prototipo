import { parseMultipart } from "./mock-multipart.mjs";

const MAX_FILE = 10 * 1024 * 1024;
const BRANCHES = ["blueMg", "componentsMg", "gigaAm", "wattsAm", "industrialMg"];
const ORIGINS = ["domestic", "international"];
const CRITICALITIES = ["high", "medium", "low"];
const STATUSES = ["approved", "rejected", "discontinued"];
const KINDS = ["presentation", "certificate", "toolDrawing", "materialDocument", "selfAssessmentAttachment", "declaration", "auditReport"];
const OWNED = { toolDrawing: "tools", materialDocument: "materials", selfAssessmentAttachment: "selfAssessments", declaration: "selfAssessments", auditReport: "audits" };
const REPLACING = ["presentation", "selfAssessmentAttachment", "declaration", "auditReport"];
const ROUTES = { tools: "tools", materials: "materials", "self-assessments": "selfAssessments", audits: "audits" };

const day = (date) => date.toISOString().slice(0, 10);
const today = () => day(new Date(Date.now() - 3 * 3600 * 1000));
const addDays = (value, days) => day(new Date(Date.parse(`${value}T00:00:00Z`) + days * 86400000));

function addMonths(value, months) {
  const date = new Date(`${value}T00:00:00Z`);
  const target = date.getUTCMonth() + months;
  const result = new Date(Date.UTC(date.getUTCFullYear() + Math.floor(target / 12), ((target % 12) + 12) % 12, 1));
  const last = new Date(Date.UTC(result.getUTCFullYear(), result.getUTCMonth() + 1, 0)).getUTCDate();
  result.setUTCDate(Math.min(date.getUTCDate(), last));
  return day(result);
}

function addYears(value, years) {
  return addMonths(value, years * 12);
}

function validCnpj(value) {
  const digits = value.replace(/\D/g, "");
  if (digits.length !== 14 || /^(\d)\1+$/.test(digits)) return false;
  const check = (length) => {
    let sum = 0;
    let weight = length - 7;
    for (let index = 0; index < length; index++) {
      sum += Number(digits[index]) * weight--;
      if (weight < 2) weight = 9;
    }
    const rest = sum % 11;
    return rest < 2 ? 0 : 11 - rest;
  };
  return check(12) === Number(digits[12]) && check(13) === Number(digits[13]);
}

const fileType = (buffer) => {
  if (buffer.subarray(0, 5).toString("latin1") === "%PDF-") return "application/pdf";
  if (buffer.subarray(0, 8).equals(Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]))) return "image/png";
  if (buffer[0] === 0xff && buffer[1] === 0xd8 && buffer[2] === 0xff) return "image/jpeg";
  return null;
};

export function createSupplierMock(users, departments) {
  let counter = 900;
  const next = () => ++counter;
  const suppliers = [];
  const now = () => new Date().toISOString();
  const nameOf = (id) => users.find((user) => user.id === id)?.name ?? "Usuário não identificado";

  const samplePdf = (title) =>
    Buffer.from(
      `%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 300 144]/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>>endobj\n4 0 obj<</Length 60>>stream\nBT /F1 14 Tf 20 70 Td (${title}) Tj ET\nendstream endobj\n5 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF`,
      "latin1",
    );

  const make = (fields, createdBy) => {
    const at = now();
    const supplier = {
      id: next(),
      ...fields,
      createdBy,
      createdAt: at,
      updatedBy: createdBy,
      updatedAt: at,
      tools: [],
      materials: [],
      selfAssessments: [],
      audits: [],
      documents: [],
    };
    suppliers.push(supplier);
    return supplier;
  };

  const base = (sapCode, name, multiBranch, criticality, origin = "domestic", taxId = "11222333000181") => ({
    sapCode,
    name,
    branch: "Matriz",
    multiBranch,
    origin,
    taxId,
    corporateName: `${name} Ltda`,
    criticality,
    zipCode: "30110-000",
    district: "Centro",
    street: "Rua das Indústrias",
    number: "100",
    contactName: "Contato Comercial",
    phone: "31 99999-0000",
    email: "contato@exemplo.com",
  });

  const selfAssessment = (supplier, assessedOn, status, needsOnSiteAudit = null) => {
    const item = {
      id: next(),
      active: true,
      responsibleId: 3,
      assessedOn,
      expiresOn: addDays(addYears(assessedOn, 1), 1),
      score: 86.5,
      status,
      editableLink: "https://exemplo.com/avaliacao",
      documentCode: "FORM-QLD-07",
      revision: 3,
      needsOnSiteAudit,
    };
    supplier.selfAssessments.push(item);
    return item;
  };

  const alfa = make(base("100200", "Plásticos Alfa", "blueMg", "high"), 1);
  selfAssessment(alfa, addDays(today(), -40), "approved", true);
  const tool = { id: next(), active: true, name: "Molde carcaça X1", assetTag: "AF-8841" };
  alfa.tools.push(tool);
  alfa.materials.push({ id: next(), active: true, sku: "ABS-0042", businessUnit: "Áudio", materialType: "Resina", criticality: "medium", hasSapSpecification: true });
  alfa.documents.push({ id: next(), kind: "presentation", ownerId: null, name: "apresentacao-alfa.pdf", contentType: "application/pdf", size: 0, version: null, expiresOn: null, uploadedAt: now(), data: samplePdf("Apresentacao Alfa") });
  alfa.documents.push({ id: next(), kind: "certificate", ownerId: null, name: "iso-9001.pdf", contentType: "application/pdf", size: 0, version: "2", expiresOn: addDays(today(), 200), uploadedAt: now(), data: samplePdf("ISO 9001") });

  const beta = make(base("100300", "Eletrônica Beta", "gigaAm", "medium", "international", "EIN 12-3456789"), 2);
  selfAssessment(beta, addDays(today(), -500), "approved");
  beta.audits.push({ id: next(), active: true, responsibleId: 3, auditedOn: addDays(today(), -400), validityMonths: 12, expiresOn: addMonths(addDays(today(), -400), 12), score: 92, status: "approved", checklistCode: "CHK-12", checklistRevision: 1 });

  make(base("100400", "Metalúrgica Gama", "industrialMg", "low"), 2);

  for (const supplier of suppliers) for (const document of supplier.documents) document.size = document.data.length;

  const qualification = (supplier) => {
    const audit = supplier.audits.filter((item) => item.active).sort((a, b) => b.auditedOn.localeCompare(a.auditedOn) || b.id - a.id)[0];
    const self = supplier.selfAssessments.filter((item) => item.active).sort((a, b) => b.assessedOn.localeCompare(a.assessedOn) || b.id - a.id)[0];
    const current = audit
      ? { kind: "onSiteAudit", status: audit.status, score: audit.score, performedOn: audit.auditedOn, expiresOn: audit.expiresOn, auditPending: false }
      : self
        ? { kind: "selfAssessment", status: self.status, score: self.score, performedOn: self.assessedOn, expiresOn: self.expiresOn, auditPending: self.needsOnSiteAudit === true }
        : null;
    return current ? { ...current, expired: current.expiresOn < today() } : null;
  };

  const summary = (supplier) => ({
    id: supplier.id,
    sapCode: supplier.sapCode,
    name: supplier.name,
    branch: supplier.branch,
    multiBranch: supplier.multiBranch,
    origin: supplier.origin,
    criticality: supplier.criticality,
    qualification: qualification(supplier),
  });

  const documentView = (document) => ({
    id: document.id,
    kind: document.kind,
    ownerId: document.ownerId,
    name: document.name,
    contentType: document.contentType,
    size: document.size,
    version: document.version,
    expiresOn: document.expiresOn,
    expired: document.expiresOn !== null && document.expiresOn < today(),
    uploadedAt: document.uploadedAt,
  });

  const withResponsible = (item) => ({ ...item, active: undefined, responsibleName: nameOf(item.responsibleId), expired: item.expiresOn < today() });

  const detail = (supplier, canEdit) => ({
    id: supplier.id,
    sapCode: supplier.sapCode,
    name: supplier.name,
    branch: supplier.branch,
    multiBranch: supplier.multiBranch,
    origin: supplier.origin,
    taxId: supplier.taxId,
    corporateName: supplier.corporateName,
    criticality: supplier.criticality,
    zipCode: supplier.zipCode,
    district: supplier.district,
    street: supplier.street,
    number: supplier.number,
    contactName: supplier.contactName,
    phone: supplier.phone,
    email: supplier.email,
    createdBy: nameOf(supplier.createdBy),
    createdAt: supplier.createdAt,
    updatedBy: nameOf(supplier.updatedBy),
    updatedAt: supplier.updatedAt,
    qualification: qualification(supplier),
    tools: supplier.tools.filter((item) => item.active).map(plain),
    materials: supplier.materials.filter((item) => item.active).map(plain),
    selfAssessments: supplier.selfAssessments.filter((item) => item.active).sort((a, b) => b.assessedOn.localeCompare(a.assessedOn)).map(withResponsible),
    audits: supplier.audits.filter((item) => item.active).sort((a, b) => b.auditedOn.localeCompare(a.auditedOn)).map(withResponsible),
    documents: [...supplier.documents].sort((a, b) => b.uploadedAt.localeCompare(a.uploadedAt)).map(documentView),
    canEdit,
  });

  const plain = (item) => {
    const copy = { ...item };
    delete copy.active;
    return copy;
  };

  const textOf = (value) => (typeof value === "string" ? value.trim() : "");
  const orNull = (value) => textOf(value) || null;

  return function handle({ req, res, url, segments, actor, json, raw, contentType, send, fail }) {
    const [, , , a, b, c, d] = segments;
    const canRegister = actor !== 3;
    const denied = () => fail(res, 403, "Acesso negado", "Sua matrícula não tem permissão para esta operação.");
    const refuse = (details, status = 400) => fail(res, status, "Falha ao salvar fornecedor", details);
    const editable = (supplier) => canRegister && (supplier.createdBy === actor || actor === 1);

    if (req.method === "GET" && !a) {
      const page = Number(url.searchParams.get("page") ?? 0);
      const size = Number(url.searchParams.get("pageSize") ?? 10);
      const search = (url.searchParams.get("search") ?? "").trim().toLowerCase();
      const origin = url.searchParams.get("origin");
      const branch = url.searchParams.get("multiBranch");
      const criticality = url.searchParams.get("criticality");
      const status = url.searchParams.get("status");

      const found = suppliers
        .filter((item) => !search || [item.name, item.sapCode, item.corporateName ?? "", item.taxId ?? ""].some((value) => value.toLowerCase().includes(search)))
        .filter((item) => !origin || item.origin === origin)
        .filter((item) => !branch || item.multiBranch === branch)
        .filter((item) => !criticality || item.criticality === criticality)
        .filter((item) => {
          if (!status) return true;
          const current = qualification(item);
          if (status === "notEvaluated") return current === null;
          if (!current) return false;
          if (status === "expired") return current.expired;
          return !current.expired && current.status === status;
        })
        .sort((x, y) => x.name.localeCompare(y.name));

      return send(res, 200, { items: found.slice(page * size, page * size + size).map(summary), totalCount: found.length, page, pageSize: size, canRegister });
    }

    if (req.method === "GET" && a === "users") {
      if (!canRegister) return denied();
      return send(res, 200, users.map((user) => ({ id: user.id, name: user.name, department: user.department ?? departments.find((item) => item.id === user.departmentId)?.name ?? "" })));
    }

    if (req.method === "POST" && !a) {
      if (!canRegister) return denied();
      const fields = readSupplier(json, null);
      if (typeof fields === "string") return refuse(fields);
      const created = make(fields, actor);
      return send(res, 201, summary(created));
    }

    const supplier = suppliers.find((candidate) => String(candidate.id) === a);
    if (!supplier) return fail(res, 404, "Falha ao buscar fornecedor", "Fornecedor não encontrado.");

    if (req.method === "GET" && !b) return send(res, 200, detail(supplier, editable(supplier)));

    if (req.method === "GET" && b === "documents" && d === "file") {
      const document = supplier.documents.find((item) => String(item.id) === c);
      if (!document) return fail(res, 404, "Falha ao abrir o documento", "Documento não encontrado.");
      res.writeHead(200, { "Content-Type": document.contentType, "Content-Disposition": `inline; filename*=UTF-8''${encodeURIComponent(document.name)}`, "Content-Length": document.data.length });
      return res.end(document.data);
    }

    if (!canRegister) return denied();
    if (!editable(supplier)) return fail(res, 403, "Acesso negado", "Somente quem cadastrou o fornecedor ou um gestor pode alterá-lo.");

    const touch = () => {
      supplier.updatedBy = actor;
      supplier.updatedAt = now();
    };

    if (req.method === "PUT" && !b) {
      const fields = readSupplier(json, supplier);
      if (typeof fields === "string") return refuse(fields);
      Object.assign(supplier, fields);
      touch();
      return send(res, 204);
    }

    if (b === "documents") {
      if (req.method === "POST" && !c) return addDocument();

      if (req.method === "DELETE" && c) {
        const document = supplier.documents.find((item) => String(item.id) === c);
        if (!document) return fail(res, 404, "Falha ao remover documento", "Documento não encontrado.");
        supplier.documents.splice(supplier.documents.indexOf(document), 1);
        touch();
        return send(res, 204);
      }
    }

    const collection = ROUTES[b];
    if (collection) {
      if (req.method === "POST" && !c) {
        const item = readItem(collection, json);
        if (typeof item === "string") return refuse(item);
        const created = { id: next(), active: true, ...item };
        supplier[collection].push(created);
        touch();
        return send(res, 201, collection === "selfAssessments" || collection === "audits" ? withResponsible(created) : plain(created));
      }

      const existing = supplier[collection].find((item) => String(item.id) === c && item.active);
      if (c && !existing) return fail(res, 404, "Falha ao salvar fornecedor", "Item não encontrado.");

      if (req.method === "PUT" && existing) {
        const item = readItem(collection, json);
        if (typeof item === "string") return refuse(item);
        Object.assign(existing, item);
        touch();
        return send(res, 204);
      }

      if (req.method === "DELETE" && existing) {
        existing.active = false;
        touch();
        return send(res, 204);
      }
    }

    return fail(res, 404, "Rota inexistente", url.pathname);

    function addDocument() {
      const fields = parseMultipart(raw, contentType);
      const value = (name) => fields.find((field) => field.name === name && !field.filename)?.content.toString().trim() ?? "";
      const kind = value("kind");
      if (!KINDS.includes(kind)) return refuse("Tipo de documento inválido.");

      const ownerRaw = value("ownerId");
      const ownerId = ownerRaw ? Number(ownerRaw) : null;
      if (kind in OWNED) {
        if (ownerId === null) return refuse("Informe o item ao qual o documento pertence.");
        if (!supplier[OWNED[kind]].some((item) => item.id === ownerId && item.active)) return fail(res, 404, "Falha ao anexar documento", "Item não encontrado.");
      } else if (ownerId !== null) {
        return refuse("Este tipo de documento não pertence a um item.");
      }

      const file = fields.find((field) => field.name === "file" && field.filename);
      if (!file) return refuse("Anexe o arquivo.");
      if (file.content.length === 0) return refuse("Não é possível enviar um arquivo vazio.");
      if (file.content.length > MAX_FILE) return refuse("O arquivo passa do tamanho máximo de 10 MB.");
      const type = fileType(file.content);
      if (!type) return refuse("O arquivo precisa ser PDF, JPG ou PNG.");

      if (REPLACING.includes(kind)) supplier.documents = supplier.documents.filter((item) => !(item.kind === kind && item.ownerId === ownerId));

      const document = {
        id: next(),
        kind,
        ownerId,
        name: file.filename,
        contentType: type,
        size: file.content.length,
        version: orNull(value("version")),
        expiresOn: orNull(value("expiresOn")),
        uploadedAt: now(),
        data: file.content,
      };
      supplier.documents.push(document);
      touch();
      return send(res, 201, documentView(document));
    }

    function readSupplier(body, existing) {
      const required = [
        ["sapCode", "Informe o código SAP do fornecedor."],
        ["name", "Informe o nome fantasia do fornecedor."],
        ["branch", "Informe a filial do fornecedor."],
        ["zipCode", "Informe o CEP."],
        ["district", "Informe o bairro."],
      ];
      for (const [key, message] of required) if (!textOf(body?.[key])) return message;
      if (!BRANCHES.includes(body.multiBranch)) return "Selecione de qual filial Multi o fornecedor é.";
      if (!ORIGINS.includes(body.origin)) return "Selecione a origem do fornecedor.";
      if (!CRITICALITIES.includes(body.criticality)) return "Selecione a criticidade.";

      const taxId = orNull(body.taxId);
      if (body.origin === "domestic" && taxId && !validCnpj(taxId)) return "O CNPJ informado não é válido.";
      const email = orNull(body.email);
      if (email && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) return "Informe um e-mail válido.";

      const sapCode = textOf(body.sapCode).toUpperCase();
      if (suppliers.some((other) => other !== existing && other.sapCode === sapCode && other.multiBranch === body.multiBranch)) {
        return `O código SAP "${sapCode}" já está cadastrado para essa filial Multi.`;
      }

      return {
        sapCode,
        name: textOf(body.name),
        branch: textOf(body.branch),
        multiBranch: body.multiBranch,
        origin: body.origin,
        taxId: body.origin === "domestic" && taxId ? taxId.replace(/\D/g, "") : taxId,
        corporateName: orNull(body.corporateName),
        criticality: body.criticality,
        zipCode: textOf(body.zipCode),
        district: textOf(body.district),
        street: orNull(body.street),
        number: orNull(body.number),
        contactName: orNull(body.contactName),
        phone: orNull(body.phone),
        email: email?.toLowerCase() ?? null,
      };
    }

    function readItem(kind, body) {
      if (kind === "tools") {
        if (!textOf(body?.name)) return "Informe o ferramental.";
        return { name: textOf(body.name), assetTag: orNull(body.assetTag) };
      }

      if (kind === "materials") {
        if (!textOf(body?.sku)) return "Informe o SKU do material.";
        if (body.criticality && !CRITICALITIES.includes(body.criticality)) return "Selecione a criticidade.";
        return {
          sku: textOf(body.sku).toUpperCase(),
          businessUnit: orNull(body.businessUnit),
          materialType: orNull(body.materialType),
          criticality: body.criticality ?? null,
          hasSapSpecification: typeof body.hasSapSpecification === "boolean" ? body.hasSapSpecification : null,
        };
      }

      if (!body?.responsibleId || !users.some((user) => user.id === body.responsibleId)) return "O responsável não existe ou está desativado.";
      if (!STATUSES.includes(body.status)) return "Selecione o status do fornecedor.";
      if (body.score !== null && body.score !== undefined && (body.score < 0 || body.score > 100)) return "A pontuação precisa estar entre 0 e 100.";

      if (kind === "selfAssessments") {
        if (!body.assessedOn) return "Informe a data.";
        if (body.editableLink && !/^https?:\/\//i.test(body.editableLink)) return "O link precisa ser um endereço http ou https válido.";
        return {
          responsibleId: body.responsibleId,
          assessedOn: body.assessedOn,
          expiresOn: addDays(addYears(body.assessedOn, 1), 1),
          score: body.score ?? null,
          status: body.status,
          editableLink: orNull(body.editableLink),
          documentCode: orNull(body.documentCode),
          revision: body.revision ?? null,
          needsOnSiteAudit: typeof body.needsOnSiteAudit === "boolean" ? body.needsOnSiteAudit : null,
        };
      }

      if (!body.auditedOn) return "Informe a data.";
      if (![3, 6, 12].includes(body.validityMonths)) return "O tempo de vencimento da auditoria precisa ser 3, 6 ou 12 meses.";
      return {
        responsibleId: body.responsibleId,
        auditedOn: body.auditedOn,
        validityMonths: body.validityMonths,
        expiresOn: addMonths(body.auditedOn, body.validityMonths),
        score: body.score ?? null,
        status: body.status,
        checklistCode: orNull(body.checklistCode),
        checklistRevision: body.checklistRevision ?? null,
      };
    }
  };
}
