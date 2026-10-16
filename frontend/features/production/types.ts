export type PostKind = "standard" | "machine" | "quality";
export type DecorationKind = "belt" | "zone" | "arrow" | "machine" | "caution" | "rack" | "table" | "pillar";
export type RunStatus = "preparing" | "running" | "closed";

export type ProductionLine = { id: number; name: string; active: boolean; createdAt: string };
export type ProductionShift = { id: number; name: string; startTime: string; endTime: string; active: boolean };
export type ProductionProduct = { id: number; code: string; name: string; active: boolean };

export type LayoutSummary = {
  id: number;
  lineId: number;
  lineName: string;
  productId: number;
  productCode: string;
  productName: string;
  shiftId: number;
  shiftName: string;
  postCount: number;
  updatedAt: string;
};

export type LayoutList = { items: LayoutSummary[]; canManage: boolean };

export type LayoutPost = {
  id: number;
  name: string;
  kind: PostKind;
  x: number;
  y: number;
  width: number;
  height: number;
  rotation: number;
  isCritical: boolean;
  targetPerHour: number | null;
  operation: string | null;
};

export type LayoutDecoration = {
  id: number;
  kind: DecorationKind;
  x: number;
  y: number;
  width: number;
  height: number;
  rotation: number;
  label: string | null;
  locked: boolean;
};

export type LayoutDetail = {
  id: number;
  lineId: number;
  lineName: string;
  productId: number;
  productCode: string;
  productName: string;
  shiftId: number;
  shiftName: string;
  posts: LayoutPost[];
  decorations: LayoutDecoration[];
  updatedAt: string;
  updatedBy: string;
  canManage: boolean;
};

export type LayoutTarget = { lineId: number; productId: number; shiftId: number };

export type SaveLayoutBody = {
  posts: (Omit<LayoutPost, "id"> & { id: number })[];
  decorations: (Omit<LayoutDecoration, "id"> & { id: number })[];
};

export type RunSummary = {
  id: number;
  status: RunStatus;
  lineId: number;
  lineName: string;
  productCode: string;
  productName: string;
  shiftName: string;
  createdAt: string;
  startedAt: string | null;
  endedAt: string | null;
  allocatedPosts: number;
  totalPosts: number;
  produced: number;
};

export type RunPage = { items: RunSummary[]; totalCount: number; page: number; pageSize: number; canSupervise: boolean };

export type RunAllocation = { userId: number; userName: string; registration: number; allocatedAt: string };

export type RunPost = {
  id: number;
  name: string;
  kind: PostKind;
  x: number;
  y: number;
  width: number;
  height: number;
  rotation: number;
  isCritical: boolean;
  targetPerHour: number | null;
  operation: string | null;
  allocation: RunAllocation | null;
  occupiedMinutes: number;
  produced: number;
  expected: number | null;
  attainment: number | null;
};

export type RunDecoration = {
  kind: DecorationKind;
  x: number;
  y: number;
  width: number;
  height: number;
  rotation: number;
  label: string | null;
};

export type RunPerson = { userId: number; userName: string; registration: number; minutes: number; produced: number; openPosts: number };

export type RunHour = { hourStart: string; produced: number };

export type RunDetail = {
  id: number;
  status: RunStatus;
  layoutId: number;
  lineId: number;
  lineName: string;
  productCode: string;
  productName: string;
  shiftName: string;
  createdAt: string;
  createdBy: string;
  startedAt: string | null;
  startedBy: string | null;
  endedAt: string | null;
  endedBy: string | null;
  posts: RunPost[];
  decorations: RunDecoration[];
  people: RunPerson[];
  hours: RunHour[];
  totalProduced: number;
  canSupervise: boolean;
  canRecord: boolean;
};

export type RunCandidate = {
  id: number;
  name: string;
  registration: number;
  department: string;
  machinePosts: number;
  holdsCommonPost: boolean;
  busyLine: string | null;
};

export type CopyLayoutResult = { created: number; updated: number };
export type CopyAllocationsResult = { allocated: number; skipped: number };

export type LayoutQuery = { lineId?: number; productId?: number; shiftId?: number };
export type RunQuery = { page?: number; pageSize?: number; status?: RunStatus; lineId?: number };
