export type SupplierOrigin = "domestic" | "international";
export type SupplierCriticality = "high" | "medium" | "low";
export type MultiBranch = "blueMg" | "componentsMg" | "gigaAm" | "wattsAm" | "industrialMg";
export type QualificationStatus = "approved" | "rejected" | "discontinued";
export type QualificationKind = "selfAssessment" | "onSiteAudit";
export type SupplierStatusFilter = "approved" | "rejected" | "discontinued" | "expired" | "notEvaluated";
export type SupplierDocumentKind =
  | "presentation"
  | "certificate"
  | "toolDrawing"
  | "materialDocument"
  | "selfAssessmentAttachment"
  | "declaration"
  | "auditReport";

export type SupplierQualification = {
  kind: QualificationKind;
  status: QualificationStatus;
  score: number | null;
  performedOn: string;
  expiresOn: string;
  expired: boolean;
  auditPending: boolean;
};

export type SupplierSummary = {
  id: number;
  sapCode: string;
  name: string;
  branch: string;
  multiBranch: MultiBranch;
  origin: SupplierOrigin;
  criticality: SupplierCriticality;
  qualification: SupplierQualification | null;
};

export type SupplierPage = {
  items: SupplierSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  canRegister: boolean;
};

export type SupplierTool = { id: number; name: string; assetTag: string | null };

export type SupplierMaterial = {
  id: number;
  sku: string;
  businessUnit: string | null;
  materialType: string | null;
  criticality: SupplierCriticality | null;
  hasSapSpecification: boolean | null;
};

export type SelfAssessment = {
  id: number;
  responsibleId: number;
  responsibleName: string;
  assessedOn: string;
  expiresOn: string;
  expired: boolean;
  score: number | null;
  status: QualificationStatus;
  editableLink: string | null;
  documentCode: string | null;
  revision: number | null;
  needsOnSiteAudit: boolean | null;
};

export type SupplierAudit = {
  id: number;
  responsibleId: number;
  responsibleName: string;
  auditedOn: string;
  validityMonths: number;
  expiresOn: string;
  expired: boolean;
  score: number | null;
  status: QualificationStatus;
  checklistCode: string | null;
  checklistRevision: number | null;
};

export type SupplierDocument = {
  id: number;
  kind: SupplierDocumentKind;
  ownerId: number | null;
  name: string;
  contentType: string;
  size: number;
  version: string | null;
  expiresOn: string | null;
  expired: boolean;
  uploadedAt: string;
};

export type SupplierDetail = {
  id: number;
  sapCode: string;
  name: string;
  branch: string;
  multiBranch: MultiBranch;
  origin: SupplierOrigin;
  taxId: string | null;
  corporateName: string | null;
  criticality: SupplierCriticality;
  zipCode: string;
  district: string;
  street: string | null;
  number: string | null;
  contactName: string | null;
  phone: string | null;
  email: string | null;
  createdBy: string;
  createdAt: string;
  updatedBy: string;
  updatedAt: string;
  qualification: SupplierQualification | null;
  tools: SupplierTool[];
  materials: SupplierMaterial[];
  selfAssessments: SelfAssessment[];
  audits: SupplierAudit[];
  documents: SupplierDocument[];
  canEdit: boolean;
};

export type SupplierUser = { id: number; name: string; department: string };

export type SupplierQuery = {
  page?: number;
  pageSize?: number;
  search?: string;
  origin?: SupplierOrigin;
  multiBranch?: MultiBranch;
  criticality?: SupplierCriticality;
  status?: SupplierStatusFilter;
};

export type SupplierBody = {
  sapCode: string;
  name: string;
  branch: string;
  multiBranch: MultiBranch;
  origin: SupplierOrigin;
  taxId: string | null;
  corporateName: string | null;
  criticality: SupplierCriticality;
  zipCode: string;
  district: string;
  street: string | null;
  number: string | null;
  contactName: string | null;
  phone: string | null;
  email: string | null;
};

export type SupplierItemRoute = "tools" | "materials" | "self-assessments" | "audits";
