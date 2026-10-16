import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type { PagedResponse } from "@/lib/http/types";
import type {
  WorkflowActionResult,
  WorkflowAssignee,
  WorkflowDetail,
  WorkflowMessage,
  WorkflowQuery,
  WorkflowStats,
  WorkflowSummary,
  WorkflowUser,
} from "../types";

type WorkflowFields = {
  title: string;
  description: string;
  priority: string;
  category: string | null;
};

class WorkflowService extends BaseService {
  constructor() {
    super("/workflows");
  }

  list(query: WorkflowQuery = {}) {
    return this.get<PagedResponse<WorkflowSummary>>(qs({ ...query, filter: query.filter === "all" ? undefined : query.filter }));
  }

  stats() {
    return this.get<WorkflowStats>("/stats");
  }

  users() {
    return this.get<WorkflowUser[]>("/users");
  }

  getById(id: number) {
    return this.get<WorkflowDetail>(`/${id}`);
  }

  create(form: FormData) {
    return this.postForm<WorkflowSummary>("", form);
  }

  update(id: number, data: WorkflowFields) {
    return this.put(`/${id}`, data);
  }

  approve(id: number, justification: string) {
    return this.post<WorkflowActionResult>(`/${id}/approve`, { justification });
  }

  reject(id: number, justification: string) {
    return this.post<WorkflowActionResult>(`/${id}/reject`, { justification });
  }

  acceptRejection(id: number, justification: string) {
    return this.post<WorkflowActionResult>(`/${id}/accept-rejection`, { justification });
  }

  returnToAssignee(id: number, stepId: number, justification: string) {
    return this.post(`/${id}/steps/${stepId}/return-to-assignee`, { justification });
  }

  returnToCreator(id: number, stepId: number, justification: string) {
    return this.post(`/${id}/steps/${stepId}/return-to-creator`, { justification });
  }

  messages(id: number, stepId: number) {
    return this.get<WorkflowMessage[]>(`/${id}/steps/${stepId}/messages`);
  }

  addAssignee(id: number, userId: number) {
    return this.post<WorkflowAssignee>(`/${id}/assignees`, { userId });
  }

  replaceAssignee(id: number, userId: number, newUserId: number) {
    return this.post<WorkflowActionResult>(`/${id}/assignees/${userId}/replace`, { newUserId });
  }

  attachment(id: number, attachmentId: number) {
    return this.stream(`/${id}/attachments/${attachmentId}`);
  }
}

export const workflowService = new WorkflowService();
