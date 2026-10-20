export type FieldType = "text" | "textArea" | "number" | "select" | "cardRadio" | "productionSpot" | "lineStop" | "defects";

export type TicketStatus = "open" | "inProgress" | "resolved" | "rejected";

export type FormOption = { value: string; label: string; icon?: string | null; color?: string | null };

export type FormDependency = { fieldId: string; values: string[] };

export type FormField = {
  id: string;
  type: FieldType;
  label: string;
  required: boolean;
  placeholder?: string | null;
  options: FormOption[];
  dependsOn?: FormDependency | null;
  templateId?: number | null;
};

export type HelpdeskForm = { fields: FormField[] };

export type Answers = Record<string, string>;

export type SectorSummary = {
  id: number;
  name: string;
  description: string | null;
  colorHex: string;
  icon: string;
  badges: string[];
  active: boolean;
  externalUrl: string | null;
  fieldCount: number;
  canAttend: boolean;
};

export type SectorList = { items: SectorSummary[]; canManage: boolean };

export type SectorDetail = {
  id: number;
  name: string;
  description: string | null;
  colorHex: string;
  icon: string;
  badges: string[];
  active: boolean;
  externalUrl: string | null;
  form: HelpdeskForm;
  resolutionForm: HelpdeskForm;
  summaryFields: string[];
  attendantRoleIds: number[];
  canAttend: boolean;
  canManage: boolean;
};

export type SaveSectorBody = {
  name: string;
  description: string | null;
  colorHex: string;
  icon: string;
  badges: string[];
  active: boolean;
  externalUrl: string | null;
  form: HelpdeskForm;
  resolutionForm: HelpdeskForm;
  summaryFields: string[];
  attendantRoleIds: number[];
};

export type FieldTemplate = {
  id: number;
  name: string;
  type: FieldType;
  label: string;
  required: boolean;
  options: FormOption[];
};

export type SaveTemplateBody = Omit<FieldTemplate, "id">;

export type HelpdeskRole = { id: number; name: string; description: string };

export type LocationPost = { id: number; name: string };

export type Location = { lineId: number; lineName: string; posts: LocationPost[] };

export type AnswerView = { label: string; type: FieldType; display: string; at: string | null };

export type TicketSummary = {
  id: number;
  sectorId: number;
  sectorName: string;
  sectorColor: string;
  sectorIcon: string;
  status: TicketStatus;
  openedByName: string;
  openedAt: string;
  lineId: number | null;
  lineName: string | null;
  postName: string | null;
  isLineStopped: boolean;
  stoppedAt: string | null;
  assignedToName: string | null;
  startedAt: string | null;
  finishedAt: string | null;
  summary: AnswerView[];
  canStart: boolean;
  canResolve: boolean;
  canReject: boolean;
};

export type StatusCounts = { open: number; inProgress: number; resolved: number; rejected: number };

export type TicketPage = {
  items: TicketSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  counts: StatusCounts;
  canAttend: boolean;
};

export type TicketDetail = {
  id: number;
  sectorId: number;
  sectorName: string;
  sectorColor: string;
  sectorIcon: string;
  status: TicketStatus;
  openedByName: string;
  openedAt: string;
  lineId: number | null;
  lineName: string | null;
  postName: string | null;
  isLineStopped: boolean;
  stoppedAt: string | null;
  defects: number;
  assignedToName: string | null;
  startedAt: string | null;
  finishedAt: string | null;
  downtimeSeconds: number | null;
  answers: AnswerView[];
  resolutionAnswers: AnswerView[];
  note: string | null;
  rejectedByName: string | null;
  rejectReason: string | null;
  canStart: boolean;
  canResolve: boolean;
  canReject: boolean;
  hasResolutionForm: boolean;
  resolutionForm: HelpdeskForm;
};

export type TicketQuery = {
  page?: number;
  pageSize?: number;
  scope?: "attend" | "mine";
  status?: string;
  sectorId?: string;
  lineId?: string;
  from?: string;
  to?: string;
};
