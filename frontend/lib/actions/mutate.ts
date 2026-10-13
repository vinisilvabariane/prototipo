import "server-only";
import { revalidatePath } from "next/cache";
import { unstable_rethrow } from "next/navigation";
import { failure, ok, type Result } from "@/lib/result";

export async function mutate(call: Promise<Result<unknown>>, ...pathsToRefresh: string[]): Promise<Result> {
  let result: Result<unknown>;
  try {
    result = await call;
  } catch (e) {
    unstable_rethrow(e);
    console.error("[action] erro inesperado", e);
    return failure("unexpected");
  }

  if (!result.ok) return result;

  for (const path of pathsToRefresh) revalidatePath(path);
  return ok();
}
