import type {
  MultiBranch,
  QualificationKind,
  QualificationStatus,
  SupplierCriticality,
  SupplierDocumentKind,
  SupplierOrigin,
  SupplierStatusFilter,
} from "./types";

export const ORIGINS: SupplierOrigin[] = ["domestic", "international"];
export const ORIGIN_LABELS: Record<SupplierOrigin, string> = { domestic: "Nacional", international: "Internacional" };

export const CRITICALITIES: SupplierCriticality[] = ["high", "medium", "low"];
export const CRITICALITY_LABELS: Record<SupplierCriticality, string> = { high: "Alta", medium: "Média", low: "Baixa" };

export const MULTI_BRANCHES: MultiBranch[] = ["blueMg", "componentsMg", "gigaAm", "wattsAm", "industrialMg"];
export const MULTI_BRANCH_LABELS: Record<MultiBranch, string> = {
  blueMg: "Blue MG",
  componentsMg: "Components MG",
  gigaAm: "Giga AM",
  wattsAm: "Watts AM",
  industrialMg: "Industrial MG",
};

export const QUALIFICATION_STATUSES: QualificationStatus[] = ["approved", "rejected", "discontinued"];
export const QUALIFICATION_STATUS_LABELS: Record<QualificationStatus, string> = {
  approved: "Aprovado",
  rejected: "Reprovado",
  discontinued: "Descontinuado",
};

export const STATUS_FILTERS: SupplierStatusFilter[] = ["approved", "rejected", "discontinued", "expired", "notEvaluated"];
export const STATUS_FILTER_LABELS: Record<SupplierStatusFilter, string> = {
  ...QUALIFICATION_STATUS_LABELS,
  expired: "Vencido",
  notEvaluated: "Não avaliado",
};

export const QUALIFICATION_KIND_LABELS: Record<QualificationKind, string> = {
  selfAssessment: "Auto avaliação",
  onSiteAudit: "Auditoria in loco",
};

export const AUDIT_VALIDITIES = [3, 6, 12] as const;

export const DOCUMENT_KIND_LABELS: Record<SupplierDocumentKind, string> = {
  presentation: "Apresentação da empresa",
  certificate: "Certificado",
  toolDrawing: "Desenho do ferramental",
  materialDocument: "Documento do material",
  selfAssessmentAttachment: "Anexo da auto avaliação",
  declaration: "Declaração",
  auditReport: "Relatório da auditoria",
};

export const DOCUMENT_KINDS = Object.keys(DOCUMENT_KIND_LABELS) as SupplierDocumentKind[];

export const MAX_DOCUMENT_BYTES = 10 * 1024 * 1024;

export const FIELD_LIMITS = {
  sapCode: 30,
  name: 150,
  branch: 100,
  taxId: 30,
  corporateName: 200,
  zipCode: 20,
  district: 100,
  street: 200,
  number: 20,
  contactName: 100,
  phone: 30,
  email: 100,
  toolName: 100,
  assetTag: 50,
  sku: 50,
  businessUnit: 50,
  materialType: 50,
  link: 500,
  code: 50,
  version: 20,
} as const;
