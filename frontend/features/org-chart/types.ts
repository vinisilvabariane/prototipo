export type OrgHolder = {
  userId: number;
  name: string;
  registration: number;
  email: string | null;
  active: boolean;
};

export type OrgPosition = {
  id: number;
  title: string;
  parentId: number | null;
  departmentId: number;
  department: string;
  holder: OrgHolder | null;
};

export type OrgChart = {
  positions: OrgPosition[];
  canManage: boolean;
};

export type OrgUser = {
  id: number;
  registration: number;
  name: string;
  department: string;
};

export type OrgNode = {
  position: OrgPosition;
  children: OrgNode[];
};
