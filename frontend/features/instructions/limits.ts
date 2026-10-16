export const MAX_STEPS = 50;

export const MAX_PDF_BYTES = 10 * 1024 * 1024;

export const MAX_IMAGE_BYTES = 5 * 1024 * 1024;

export const FIELD_LIMITS = {
  title: 150,
  documentNumber: 50,
  product: 100,
  preparedBy: 100,
  approvedBy: 100,
  requiredPpe: 1000,
  processSpecifications: 2000,
  notes: 1000,
  stepText: 2000,
} as const;

export const FIELD_LABELS = {
  title: "o título",
  documentNumber: "o número do documento",
  product: "o produto",
  preparedBy: "quem elaborou",
  approvedBy: "quem aprovou",
  requiredPpe: "os EPIs obrigatórios",
  processSpecifications: "as especificações de processo",
} as const;
