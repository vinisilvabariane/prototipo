import { BaseService } from "@/lib/http/BaseService";
import type { OrgChart, OrgPosition, OrgUser } from "../types";

type CreateInput = { title: string; departmentId: number; parentId: number | null; holderId: number | null };

type UpdateInput = { title: string; departmentId: number };

class OrgChartService extends BaseService {
  constructor() {
    super("/org-chart");
  }

  chart() {
    return this.get<OrgChart>();
  }

  assignableUsers() {
    return this.get<OrgUser[]>("/assignable-users");
  }

  create(data: CreateInput) {
    return this.post<OrgPosition>("/positions", data);
  }

  update(id: number, data: UpdateInput) {
    return this.put(`/positions/${id}`, data);
  }

  assignHolder(id: number, userId: number | null) {
    return this.put(`/positions/${id}/holder`, { userId });
  }

  move(id: number, parentId: number | null) {
    return this.put(`/positions/${id}/parent`, { parentId });
  }

  remove(id: number) {
    return this.del(`/positions/${id}`);
  }
}

export const orgChartService = new OrgChartService();
