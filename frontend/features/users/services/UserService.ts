import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type { Role } from "@/features/roles/types";
import type { PagedResponse } from "@/lib/http/types";
import type { CreateUserInput, User, UserInput } from "../types";

class UserService extends BaseService {
  constructor() {
    super("/users");
  }

  list(query: { page?: number; pageSize?: number; search?: string } = {}) {
    return this.get<PagedResponse<User>>(qs(query));
  }

  getById(id: number) {
    return this.get<User>(`/${id}`);
  }

  create(data: CreateUserInput) {
    return this.post<User>("", data);
  }

  update(id: number, data: UserInput) {
    return this.put(`/${id}`, data);
  }

  remove(id: number) {
    return this.del(`/${id}`);
  }

  roles(userId: number) {
    return this.get<Role[]>(`/roles/${userId}`);
  }
}

export const userService = new UserService();
