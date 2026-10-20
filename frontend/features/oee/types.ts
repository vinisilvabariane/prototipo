import type { PostKind, RunStatus } from "@/features/production/types";

export type OeePillars = { availability: number; performance: number; quality: number; oee: number };

export type OeeHour = { hourStart: string; seconds: number; pillars: OeePillars; produced: number; theoretical: number };

export type OeePost = {
  postId: number;
  name: string;
  kind: PostKind;
  x: number;
  targetPerHour: number | null;
  measured: boolean;
  pillars: OeePillars;
  produced: number;
  theoretical: number;
  occupiedSeconds: number;
  downtimeSeconds: number;
  defects: number;
  hours: OeeHour[];
};

export type RunOee = {
  runId: number;
  status: RunStatus;
  lineId: number;
  lineName: string;
  productCode: string;
  productName: string;
  shiftName: string;
  startedAt: string | null;
  endedAt: string | null;
  elapsedSeconds: number;
  line: OeePillars;
  produced: number;
  theoretical: number;
  lastPostId: number | null;
  posts: OeePost[];
  hours: OeeHour[];
  lossSourceAvailable: boolean;
  formulaSetName: string;
};

export type OeeRunSummary = {
  runId: number;
  status: RunStatus;
  lineId: number;
  lineName: string;
  productCode: string;
  productName: string;
  shiftName: string;
  startedAt: string | null;
  endedAt: string | null;
  elapsedSeconds: number;
  pillars: OeePillars;
  produced: number;
  theoretical: number;
  measuredPosts: number;
  totalPosts: number;
};

export type OeeComparison = {
  from: string;
  to: string;
  runs: OeeRunSummary[];
  lossSourceAvailable: boolean;
  formulaSetName: string;
};

export type OeeQuery = { from?: string; to?: string; status?: string; lineId?: string };

export type SlotKey = "availability" | "performance" | "quality" | "machineOee" | "lineAvailability" | "linePerformance" | "lineQuality" | "lineOee";

export type OeeFormulas = Record<SlotKey, string>;

export type OeeFormulaSet = {
  id: number;
  name: string;
  isActive: boolean;
  formulas: OeeFormulas;
  updatedAt: string;
  updatedByName: string | null;
};

export type OeeVariable = { key: string; label: string; description: string; unit: string; slots: SlotKey[] };

export type OeeSlot = { key: SlotKey; label: string; group: "post" | "line" };

export type OeeFunction = { name: string; signature: string; description: string };

export type OeeConfig = {
  sets: OeeFormulaSet[];
  activeSetId: number | null;
  default: OeeFormulas;
  variables: OeeVariable[];
  slots: OeeSlot[];
  functions: OeeFunction[];
  lossSourceAvailable: boolean;
  canManage: boolean;
};

export type SaveFormulaSetBody = { name: string; formulas: OeeFormulas; activate: boolean };

export type TestFormulaBody = {
  slot: SlotKey;
  expression: string;
  runId?: number;
  postId?: number;
  values?: Record<string, number>;
};

export type FormulaTest = {
  valid: boolean;
  error: string | null;
  position: number | null;
  identifiers: string[];
  result: number | null;
  values: Record<string, number>;
};
