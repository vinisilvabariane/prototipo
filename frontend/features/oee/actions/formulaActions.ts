"use server";

import { revalidatePath } from "next/cache";
import { mutate } from "@/lib/actions/mutate";
import type { Result } from "@/lib/result";
import { oeeService } from "../services/OeeService";
import type { FormulaTest, OeeFormulaSet, SaveFormulaSetBody, TestFormulaBody } from "../types";

const refresh = ["/oee/formulas", "/oee", "/oee/live"];

export async function createFormulaSet(body: SaveFormulaSetBody): Promise<Result<OeeFormulaSet>> {
  const result = await oeeService.createSet(body);
  if (result.ok) for (const path of refresh) revalidatePath(path);
  return result;
}

export async function updateFormulaSet(id: number, body: SaveFormulaSetBody): Promise<Result> {
  return mutate(oeeService.updateSet(id, body), ...refresh);
}

export async function deleteFormulaSet(id: number): Promise<Result> {
  return mutate(oeeService.deleteSet(id), ...refresh);
}

export async function activateFormulaSet(id: number | null): Promise<Result> {
  return mutate(oeeService.setActive(id), ...refresh);
}

export async function testFormula(body: TestFormulaBody): Promise<Result<FormulaTest>> {
  return oeeService.test(body);
}
