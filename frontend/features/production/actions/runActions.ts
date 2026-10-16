"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { integer } from "@/lib/actions/form";
import { mutate } from "@/lib/actions/mutate";
import { failure, ok, type Result } from "@/lib/result";
import { productionService } from "../services/ProductionService";
import type { CopyAllocationsResult, RunCandidate } from "../types";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

const MAX_QUANTITY = 1_000_000;

const refresh = (runId: number) => [`/production/runs/${runId}`, "/production"];

export async function createRun(layoutId: number): Promise<Result> {
  const result = await productionService.createRun(layoutId);
  if (!result.ok) return result;

  revalidatePath("/production");
  redirect(`/production/runs/${result.value.id}`);
}

export async function createRunFromForm(_prev: Result | undefined, formData: FormData): Promise<Result> {
  const layoutId = integer(formData, "layoutId");
  if (!layoutId) return invalid("Selecione o layout da produção.");

  return createRun(layoutId);
}

export async function searchCandidates(runId: number, search: string): Promise<Result<RunCandidate[]>> {
  return productionService.candidates(runId, search.trim());
}

export async function allocatePost(runId: number, postId: number, userId: number): Promise<Result> {
  return mutate(productionService.allocate(runId, postId, userId), ...refresh(runId));
}

export async function releasePost(runId: number, postId: number): Promise<Result> {
  return mutate(productionService.release(runId, postId), ...refresh(runId));
}

export async function releaseAllPosts(runId: number): Promise<Result> {
  return mutate(productionService.releaseAll(runId), ...refresh(runId));
}

export async function copyPreviousAllocations(runId: number): Promise<Result<CopyAllocationsResult>> {
  const result = await productionService.copyPrevious(runId);
  if (!result.ok) return result;

  for (const path of refresh(runId)) revalidatePath(path);
  return ok(result.value);
}

export async function startRun(runId: number): Promise<Result> {
  return mutate(productionService.start(runId), ...refresh(runId));
}

export async function closeRun(runId: number): Promise<Result> {
  return mutate(productionService.close(runId), ...refresh(runId));
}

export async function cancelRun(runId: number): Promise<Result> {
  const result = await productionService.cancel(runId);
  if (!result.ok) return result;

  revalidatePath("/production");
  redirect("/production");
}

export async function recordOutput(runId: number, postId: number, quantity: number, hourStart: string | null): Promise<Result> {
  if (!Number.isInteger(quantity) || quantity < 0 || quantity > MAX_QUANTITY) {
    return invalid("A quantidade precisa ser um número inteiro entre 0 e 1.000.000.");
  }

  return mutate(productionService.recordOutput(runId, postId, quantity, hourStart), ...refresh(runId));
}

export async function refreshRun(runId: number): Promise<void> {
  for (const path of refresh(runId)) revalidatePath(path);
}
