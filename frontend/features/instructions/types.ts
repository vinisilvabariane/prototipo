export type InstructionSummary = {
  id: number;
  code: string;
  title: string;
  documentNumber: string;
  product: string;
  preparedBy: string;
  approvedBy: string;
  stepCount: number;
  pdfSize: number;
  createdAt: string;
  updatedAt: string;
};

export type InstructionStep = {
  id: number;
  position: number;
  text: string;
  hasImage: boolean;
};

export type InstructionDetail = Omit<InstructionSummary, "stepCount"> & {
  requiredPpe: string;
  processSpecifications: string;
  notes: string | null;
  createdBy: string;
  updatedBy: string;
  steps: InstructionStep[];
  canManage: boolean;
};

export type InstructionPage = {
  items: InstructionSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  canManage: boolean;
};

export type InstructionQuery = { page?: number; pageSize?: number; search?: string };
