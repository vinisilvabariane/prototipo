import { BaseService } from "@/lib/http/BaseService";
import type { Department } from "../types";

class DepartmentService extends BaseService {
  constructor() {
    super("/departments");
  }

  all() {
    return this.get<Department[]>("/all");
  }

  create(name: string) {
    return this.post<Department>("", { name });
  }

  update(id: number, name: string) {
    return this.put(`/${id}`, { name });
  }

  remove(id: number) {
    return this.del(`/${id}`);
  }
}

export const departmentService = new DepartmentService();
