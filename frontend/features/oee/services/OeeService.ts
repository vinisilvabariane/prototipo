import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type { FormulaTest, OeeComparison, OeeConfig, OeeFormulaSet, OeeQuery, RunOee, SaveFormulaSetBody, TestFormulaBody } from "../types";

class OeeService extends BaseService {
  constructor() {
    super("/oee");
  }

  runs(query: OeeQuery = {}) {
    return this.get<OeeComparison>(`/runs${qs(query)}`);
  }

  run(id: number) {
    return this.get<RunOee>(`/runs/${id}`);
  }

  config() {
    return this.get<OeeConfig>("/config");
  }

  createSet(body: SaveFormulaSetBody) {
    return this.post<OeeFormulaSet>("/formulas", body);
  }

  updateSet(id: number, body: SaveFormulaSetBody) {
    return this.put(`/formulas/${id}`, body);
  }

  deleteSet(id: number) {
    return this.del(`/formulas/${id}`);
  }

  setActive(setId: number | null) {
    return this.put("/formulas/active", { setId });
  }

  test(body: TestFormulaBody) {
    return this.post<FormulaTest>("/formulas/test", body);
  }
}

export const oeeService = new OeeService();
