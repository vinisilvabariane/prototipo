export type WorkflowStatus = "inProgress" | "approved" | "rejected";

export type WorkflowPriority = "high" | "medium" | "low";

export type WorkflowCategory = "requirements" | "standards" | "procedures";

export type WorkflowAction =
  | "created"
  | "edited"
  | "approval"
  | "rejection"
  | "finalApproval"
  | "finalRejection"
  | "assigneeAdded"
  | "assigneeReplaced";

export type ReviewStage = "none" | "creatorReview" | "assigneeReview";

export type ReviewRole = "none" | "creator" | "assignee";

export type WorkflowFilter = "all" | "mine" | "participating";

export type WorkflowSummary = {
  id: number;
  title: string;
  description: string;
  priority: WorkflowPriority;
  category: WorkflowCategory | null;
  status: WorkflowStatus;
  createdAt: string;
  dueDate: string | null;
  completedAt: string | null;
};

export type WorkflowAssignee = {
  userId: number;
  name: string;
  registration: number;
  assignedAt: string;
  approved: boolean;
};

export type WorkflowAttachment = {
  id: number;
  name: string;
  contentType: string;
  size: number;
  uploadedAt: string;
};

export type WorkflowStep = {
  id: number;
  action: WorkflowAction;
  description: string;
  justification: string | null;
  userId: number;
  userName: string;
  createdAt: string;
  review: ReviewStage;
  actorRole: ReviewRole;
};

export type WorkflowPermissions = {
  isCreator: boolean;
  isAssignee: boolean;
  canApprove: boolean;
  canReject: boolean;
  canEdit: boolean;
  canManageAssignees: boolean;
};

export type WorkflowDetail = WorkflowSummary & {
  creatorId: number;
  creatorName: string;
  assignees: WorkflowAssignee[];
  attachments: WorkflowAttachment[];
  steps: WorkflowStep[];
  permissions: WorkflowPermissions;
};

export type WorkflowMessage = {
  id: number;
  userId: number;
  userName: string;
  text: string;
  createdAt: string;
};

export type WorkflowStats = {
  total: number;
  inProgress: number;
  approved: number;
  rejected: number;
};

export type WorkflowUser = { id: number; registration: number; name: string };

export type WorkflowActionResult = { status: WorkflowStatus; message: string };

export type WorkflowQuery = {
  page?: number;
  pageSize?: number;
  search?: string;
  filter?: WorkflowFilter;
};
