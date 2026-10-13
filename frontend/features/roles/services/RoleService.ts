import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type { PagedResponse } from "@/lib/http/types";
import type { Role, RoleInput } from "../types";

class RoleService extends BaseService {
  constructor() {
    super("/roles");
  }

  list(page = 0) {
    return this.get<PagedResponse<Role>>(qs({ page }));
  }

  all() {
    return this.get<Role[]>("/all");
  }

  getById(id: number) {
    return this.get<Role>(`/${id}`);
  }

  create(data: RoleInput) {
    return this.post<Role>("", data);
  }

  update(id: number, data: RoleInput) {
    return this.put(`/${id}`, data);
  }

  remove(id: number) {
    return this.del(`/${id}`);
  }

  assign(userId: number, roleId: number) {
    return this.post("/set-role", { userId, roleId });
  }

  revoke(userId: number, roleId: number) {
    return this.post("/revoke-role", { userId, roleId });
  }
}

export const roleService = new RoleService();
