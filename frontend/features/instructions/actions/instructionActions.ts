"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { text } from "@/lib/actions/form";
import { failure, type Result } from "@/lib/result";
import { FIELD_LABELS, FIELD_LIMITS, MAX_IMAGE_BYTES, MAX_PDF_BYTES, MAX_STEPS } from "../limits";
import { instructionService } from "../services/InstructionService";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

function problemsOf(formData: FormData, creating: boolean) {
  const problems: string[] = [];

  if (!text(formData, "code")) problems.push("Informe o código da instrução.");

  for (const key of Object.keys(FIELD_LABELS) as (keyof typeof FIELD_LABELS)[]) {
    const value = text(formData, key);
    if (!value) problems.push(`Informe ${FIELD_LABELS[key]}.`);
    else if (value.length > FIELD_LIMITS[key]) problems.push(`O campo ${FIELD_LABELS[key]} não pode passar de ${FIELD_LIMITS[key]} caracteres.`);
  }

  if (text(formData, "notes").length > FIELD_LIMITS.notes) problems.push(`As observações não podem passar de ${FIELD_LIMITS.notes} caracteres.`);

  const texts = [...formData.keys()].filter((key) => /^steps\[\d+\]\.text$/.test(key));
  if (texts.length > MAX_STEPS) problems.push(`A instrução pode ter no máximo ${MAX_STEPS} passos.`);
  if (texts.some((key) => !String(formData.get(key) ?? "").trim())) problems.push("Todo passo precisa de uma instrução escrita.");

  const pdf = formData.get("pdf");
  if (creating && !(pdf instanceof File && pdf.size > 0)) problems.push("Anexe ou gere o PDF da instrução.");
  if (pdf instanceof File && pdf.size > MAX_PDF_BYTES) problems.push("O PDF passa do tamanho máximo de 10 MB.");

  for (const [key, value] of formData.entries()) {
    if (/^stepImages\[\d+\]$/.test(key) && value instanceof File && value.size > MAX_IMAGE_BYTES) {
      problems.push(`A imagem "${value.name}" passa do tamanho máximo de 5 MB.`);
    }
  }

  return problems;
}

export async function saveInstruction(id: number | null, _prev: Result | undefined, formData: FormData): Promise<Result> {
  const problems = problemsOf(formData, id === null);
  if (problems.length > 0) return invalid(...problems);

  const result = id === null ? await instructionService.create(formData) : await instructionService.update(id, formData);
  if (!result.ok) return result;

  const target = id ?? (result.value as { id: number }).id;

  revalidatePath("/instructions");
  revalidatePath(`/instructions/${target}`);
  redirect(`/instructions/${target}`);
}

export async function deleteInstruction(id: number): Promise<Result> {
  const result = await instructionService.remove(id);
  if (!result.ok) return result;

  revalidatePath("/instructions");
  redirect("/instructions");
}
