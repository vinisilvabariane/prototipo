"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { integer, text } from "@/lib/actions/form";
import { mutate } from "@/lib/actions/mutate";
import { failure, type Result } from "@/lib/result";
import {
  AUDIT_VALIDITIES,
  CRITICALITIES,
  DOCUMENT_KINDS,
  FIELD_LIMITS,
  MAX_DOCUMENT_BYTES,
  MULTI_BRANCHES,
  ORIGINS,
  QUALIFICATION_STATUSES,
} from "../labels";
import { supplierService } from "../services/SupplierService";
import type { SupplierBody, SupplierCriticality, SupplierDocumentKind, SupplierItemRoute } from "../types";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

const optional = (formData: FormData, key: string) => text(formData, key) || null;

const tooLong = (value: string | null, limit: number, label: string, problems: string[]) => {
  if (value && value.length > limit) problems.push(`${label} não pode passar de ${limit} caracteres.`);
};

function scoreOf(formData: FormData, problems: string[]) {
  const raw = text(formData, "score").replace(",", ".");
  if (!raw) return null;

  const value = Number(raw);
  if (!Number.isFinite(value) || value < 0 || value > 100) {
    problems.push("A pontuação precisa estar entre 0 e 100.");
    return null;
  }
  return value;
}

function revisionOf(formData: FormData, key: string, problems: string[]) {
  const raw = text(formData, key);
  if (!raw) return null;

  const value = Number(raw);
  if (!Number.isInteger(value) || value < 0 || value > 999) {
    problems.push("O número da revisão precisa estar entre 0 e 999.");
    return null;
  }
  return value;
}

function triState(formData: FormData, key: string) {
  const value = text(formData, key);
  return value === "true" ? true : value === "false" ? false : null;
}

function supplierBody(formData: FormData) {
  const problems: string[] = [];
  const required = (key: "sapCode" | "name" | "branch" | "zipCode" | "district", label: string) => {
    const value = text(formData, key);
    if (!value) problems.push(`Informe ${label}.`);
    tooLong(value, FIELD_LIMITS[key], `O campo ${label}`, problems);
    return value;
  };

  const sapCode = required("sapCode", "o código SAP");
  const name = required("name", "o nome fantasia");
  const branch = required("branch", "a filial do fornecedor");
  const zipCode = required("zipCode", "o CEP");
  const district = required("district", "o bairro");

  const multiBranch = text(formData, "multiBranch");
  const origin = text(formData, "origin");
  const criticality = text(formData, "criticality");

  if (!MULTI_BRANCHES.some((item) => item === multiBranch)) problems.push("Selecione de qual filial Multi o fornecedor é.");
  if (!ORIGINS.some((item) => item === origin)) problems.push("Selecione a origem do fornecedor.");
  if (!CRITICALITIES.some((item) => item === criticality)) problems.push("Selecione a criticidade.");

  const optionals = {
    taxId: optional(formData, "taxId"),
    corporateName: optional(formData, "corporateName"),
    street: optional(formData, "street"),
    number: optional(formData, "number"),
    contactName: optional(formData, "contactName"),
    phone: optional(formData, "phone"),
    email: optional(formData, "email"),
  };

  tooLong(optionals.taxId, FIELD_LIMITS.taxId, "O CNPJ", problems);
  tooLong(optionals.corporateName, FIELD_LIMITS.corporateName, "A razão social", problems);
  tooLong(optionals.street, FIELD_LIMITS.street, "O logradouro", problems);
  tooLong(optionals.number, FIELD_LIMITS.number, "O número", problems);
  tooLong(optionals.contactName, FIELD_LIMITS.contactName, "O responsável", problems);
  tooLong(optionals.phone, FIELD_LIMITS.phone, "O telefone", problems);
  tooLong(optionals.email, FIELD_LIMITS.email, "O e-mail", problems);

  const body = { sapCode, name, branch, multiBranch, origin, criticality, zipCode, district, ...optionals } as SupplierBody;
  return { body, problems };
}

export async function saveSupplier(id: number | null, _prev: Result | undefined, formData: FormData): Promise<Result> {
  const { body, problems } = supplierBody(formData);
  if (problems.length > 0) return invalid(...problems);

  const result = id === null ? await supplierService.create(body) : await supplierService.update(id, body);
  if (!result.ok) return result;

  const target = id ?? (result.value as { id: number }).id;

  revalidatePath("/suppliers");
  revalidatePath(`/suppliers/${target}`);
  redirect(`/suppliers/${target}`);
}

async function saveItem(supplierId: number, route: SupplierItemRoute, itemId: number | null, body: unknown) {
  const call = itemId === null ? supplierService.addItem(supplierId, route, body) : supplierService.updateItem(supplierId, route, itemId, body);
  return mutate(call, `/suppliers/${supplierId}`, "/suppliers");
}

export async function saveTool(supplierId: number, itemId: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const name = text(formData, "name");
  const assetTag = optional(formData, "assetTag");

  if (!name) problems.push("Informe o ferramental.");
  tooLong(name, FIELD_LIMITS.toolName, "O ferramental", problems);
  tooLong(assetTag, FIELD_LIMITS.assetTag, "O ativo fixo", problems);
  if (problems.length > 0) return invalid(...problems);

  return saveItem(supplierId, "tools", itemId, { name, assetTag });
}

export async function saveMaterial(supplierId: number, itemId: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const sku = text(formData, "sku");
  const businessUnit = optional(formData, "businessUnit");
  const materialType = optional(formData, "materialType");
  const criticality = optional(formData, "criticality");

  if (!sku) problems.push("Informe o SKU do material.");
  tooLong(sku, FIELD_LIMITS.sku, "O SKU", problems);
  tooLong(businessUnit, FIELD_LIMITS.businessUnit, "A BU", problems);
  tooLong(materialType, FIELD_LIMITS.materialType, "O tipo de material", problems);
  if (criticality && !CRITICALITIES.some((item) => item === criticality)) problems.push("Selecione a criticidade.");
  if (problems.length > 0) return invalid(...problems);

  return saveItem(supplierId, "materials", itemId, {
    sku,
    businessUnit,
    materialType,
    criticality: criticality as SupplierCriticality | null,
    hasSapSpecification: triState(formData, "hasSapSpecification"),
  });
}

function evaluationFields(formData: FormData, dateKey: string, problems: string[]) {
  const responsibleId = integer(formData, "responsibleId");
  const date = text(formData, dateKey);
  const status = text(formData, "status");

  if (!responsibleId) problems.push("Selecione o responsável pela validação.");
  if (!date) problems.push("Informe a data.");
  if (!QUALIFICATION_STATUSES.some((item) => item === status)) problems.push("Selecione o status do fornecedor.");

  return { responsibleId, date, status, score: scoreOf(formData, problems) };
}

export async function saveSelfAssessment(supplierId: number, itemId: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const { responsibleId, date, status, score } = evaluationFields(formData, "assessedOn", problems);
  const editableLink = optional(formData, "editableLink");
  const documentCode = optional(formData, "documentCode");

  tooLong(editableLink, FIELD_LIMITS.link, "O link", problems);
  tooLong(documentCode, FIELD_LIMITS.code, "O registro do manual", problems);
  if (editableLink && !/^https?:\/\//i.test(editableLink)) problems.push("O link precisa ser um endereço http ou https válido.");
  const revision = revisionOf(formData, "revision", problems);
  if (problems.length > 0) return invalid(...problems);

  return saveItem(supplierId, "self-assessments", itemId, {
    responsibleId,
    assessedOn: date,
    score,
    status,
    editableLink,
    documentCode,
    revision,
    needsOnSiteAudit: triState(formData, "needsOnSiteAudit"),
  });
}

export async function saveAudit(supplierId: number, itemId: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const { responsibleId, date, status, score } = evaluationFields(formData, "auditedOn", problems);
  const validityMonths = Number(text(formData, "validityMonths"));
  const checklistCode = optional(formData, "checklistCode");

  if (!AUDIT_VALIDITIES.some((item) => item === validityMonths)) problems.push("O tempo de vencimento da auditoria precisa ser 3, 6 ou 12 meses.");
  tooLong(checklistCode, FIELD_LIMITS.code, "O registro do checklist", problems);
  const checklistRevision = revisionOf(formData, "checklistRevision", problems);
  if (problems.length > 0) return invalid(...problems);

  return saveItem(supplierId, "audits", itemId, {
    responsibleId,
    auditedOn: date,
    validityMonths,
    score,
    status,
    checklistCode,
    checklistRevision,
  });
}

export async function removeSupplierItem(supplierId: number, route: SupplierItemRoute, itemId: number): Promise<Result> {
  return mutate(supplierService.removeItem(supplierId, route, itemId), `/suppliers/${supplierId}`, "/suppliers");
}

export async function uploadDocument(supplierId: number, _prev: Result | undefined, formData: FormData): Promise<Result> {
  const problems: string[] = [];
  const kind = text(formData, "kind");
  const ownerId = integer(formData, "ownerId");
  const version = optional(formData, "version");
  const expiresOn = optional(formData, "expiresOn");
  const file = formData.get("file");

  if (!DOCUMENT_KINDS.some((item) => item === kind)) problems.push("Tipo de documento inválido.");
  tooLong(version, FIELD_LIMITS.version, "A versão", problems);

  if (!(file instanceof File) || file.size === 0) problems.push("Anexe o arquivo.");
  else if (file.size > MAX_DOCUMENT_BYTES) problems.push("O arquivo passa do tamanho máximo de 10 MB.");
  if (problems.length > 0 || !(file instanceof File)) return invalid(...problems);

  const body = new FormData();
  body.set("kind", kind as SupplierDocumentKind);
  if (ownerId) body.set("ownerId", String(ownerId));
  if (version) body.set("version", version);
  if (expiresOn) body.set("expiresOn", expiresOn);
  body.set("file", file, file.name);

  return mutate(supplierService.addDocument(supplierId, body), `/suppliers/${supplierId}`);
}

export async function removeSupplierDocument(supplierId: number, documentId: number): Promise<Result> {
  return mutate(supplierService.removeDocument(supplierId, documentId), `/suppliers/${supplierId}`);
}
