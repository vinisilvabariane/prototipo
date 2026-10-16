import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type { InstructionDetail, InstructionPage, InstructionQuery, InstructionSummary } from "../types";

class InstructionService extends BaseService {
  constructor() {
    super("/work-instructions");
  }

  list(query: InstructionQuery = {}) {
    return this.get<InstructionPage>(qs(query));
  }

  getById(id: number) {
    return this.get<InstructionDetail>(`/${id}`);
  }

  nextCode() {
    return this.get<{ code: string }>("/next-code");
  }

  create(form: FormData) {
    return this.postForm<InstructionSummary>("", form);
  }

  update(id: number, form: FormData) {
    return this.putForm(`/${id}`, form);
  }

  remove(id: number) {
    return this.del(`/${id}`);
  }

  pdf(id: number) {
    return this.stream(`/${id}/pdf`);
  }

  stepImage(id: number, stepId: number) {
    return this.stream(`/${id}/steps/${stepId}/image`);
  }
}

export const instructionService = new InstructionService();
