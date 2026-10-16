"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { integer } from "@/lib/actions/form";
import { failure, ok, type Result } from "@/lib/result";
import { productionService } from "../services/ProductionService";
import type { CopyLayoutResult, LayoutDetail, LayoutTarget, SaveLayoutBody } from "../types";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

function targetOf(formData: FormData) {
  const lineId = integer(formData, "lineId");
  const productId = integer(formData, "productId");
  const shiftId = integer(formData, "shiftId");
  const problems: string[] = [];

  if (!lineId) problems.push("Selecione a linha.");
  if (!productId) problems.push("Selecione o produto.");
  if (!shiftId) problems.push("Selecione o turno.");

  return { target: { lineId, productId, shiftId }, problems };
}

export async function createLayout(_prev: Result | undefined, formData: FormData): Promise<Result> {
  const { target, problems } = targetOf(formData);
  if (problems.length > 0) return invalid(...problems);

  const result = await productionService.createLayout(target);
  if (!result.ok) return result;

  revalidatePath("/production/layouts");
  redirect(`/production/layouts/${result.value.id}`);
}

export async function saveLayout(id: number, body: SaveLayoutBody): Promise<Result<LayoutDetail>> {
  const saved = await productionService.saveLayout(id, body);
  if (!saved.ok) return saved;

  revalidatePath("/production/layouts");
  revalidatePath(`/production/layouts/${id}`);
  return productionService.layout(id);
}

export async function deleteLayout(id: number): Promise<Result> {
  const result = await productionService.deleteLayout(id);
  if (!result.ok) return result;

  revalidatePath("/production/layouts");
  redirect("/production/layouts");
}

export async function copyLayout(id: number, targets: LayoutTarget[]): Promise<Result<CopyLayoutResult>> {
  if (targets.length === 0) return invalid("Selecione ao menos um destino para a cópia.");

  const result = await productionService.copyLayout(id, targets);
  if (!result.ok) return result;

  revalidatePath("/production/layouts");
  return ok(result.value);
}
