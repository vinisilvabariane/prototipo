"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { mutate } from "@/lib/actions/mutate";
import { integer, text } from "@/lib/actions/form";
import { failure, ok, type Failure, type Result } from "@/lib/result";
import { todayInZone } from "../format";
import {
  CATEGORIES,
  MAX_ATTACHMENT_BYTES,
  MAX_ATTACHMENTS,
  MAX_DESCRIPTION_LENGTH,
  MAX_JUSTIFICATION_LENGTH,
  MAX_TITLE_LENGTH,
  PRIORITIES,
} from "../labels";
import { workflowService } from "../services/WorkflowService";
import type { WorkflowMessage } from "../types";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

function justification(formData: FormData): string | Failure {
  const value = text(formData, "justification");
  if (!value) return invalid("Informe uma justificativa.");
  if (value.length > MAX_JUSTIFICATION_LENGTH) {
    return invalid(`A justificativa não pode passar de ${MAX_JUSTIFICATION_LENGTH} caracteres.`);
  }
  return value;
}

function workflowFields(formData: FormData) {
  const title = text(formData, "title");
  const description = text(formData, "description");
  const priority = text(formData, "priority");
  const category = text(formData, "category");

  const problems: string[] = [];
  if (!title) problems.push("Informe o título do workflow.");
  if (title.length > MAX_TITLE_LENGTH) problems.push(`O título não pode passar de ${MAX_TITLE_LENGTH} caracteres.`);
  if (description.length > MAX_DESCRIPTION_LENGTH) {
    problems.push(`A descrição não pode passar de ${MAX_DESCRIPTION_LENGTH} caracteres.`);
  }
  if (!PRIORITIES.some((item) => item === priority)) problems.push("Selecione uma prioridade.");
  if (category && !CATEGORIES.some((item) => item === category)) problems.push("Selecione uma categoria válida.");

  return { fields: { title, description, priority, category: category || null }, problems };
}

export async function createWorkflow(_prev: Result | undefined, formData: FormData): Promise<Result> {
  const { fields, problems } = workflowFields(formData);
  const dueDate = text(formData, "dueDate");

  if (!dueDate) problems.push("Informe o prazo do workflow.");
  else if (dueDate < todayInZone()) problems.push("A data do prazo final não pode ser menor que a data atual.");

  const files = formData.getAll("attachments").filter((entry): entry is File => entry instanceof File && entry.size > 0);
  if (files.length > MAX_ATTACHMENTS) problems.push(`Você pode enviar no máximo ${MAX_ATTACHMENTS} arquivos.`);
  for (const file of files) {
    if (file.size > MAX_ATTACHMENT_BYTES) problems.push(`O arquivo "${file.name}" excede o tamanho máximo de 10 MB.`);
  }

  if (problems.length > 0) return invalid(...problems);

  const body = new FormData();
  body.set("title", fields.title);
  body.set("description", fields.description);
  body.set("priority", fields.priority);
  if (fields.category) body.set("category", fields.category);
  body.set("dueDate", dueDate);
  for (const id of formData.getAll("assigneeIds")) body.append("assigneeIds", String(id));
  for (const file of files) body.append("attachments", file, file.name);

  const result = await workflowService.create(body);
  if (!result.ok) return result;

  revalidatePath("/workflows");
  redirect(`/workflows/${result.value.id}`);
}

export async function updateWorkflow(id: number, _prev: Result | undefined, formData: FormData) {
  const { fields, problems } = workflowFields(formData);
  if (problems.length > 0) return invalid(...problems);

  return mutate(workflowService.update(id, fields), `/workflows/${id}`, "/workflows");
}

export async function approveWorkflow(id: number, _prev: Result | undefined, formData: FormData) {
  const reason = justification(formData);
  if (typeof reason !== "string") return reason;

  return mutate(workflowService.approve(id, reason), `/workflows/${id}`, "/workflows");
}

export async function rejectWorkflow(id: number, _prev: Result | undefined, formData: FormData) {
  const reason = justification(formData);
  if (typeof reason !== "string") return reason;

  return mutate(workflowService.reject(id, reason), `/workflows/${id}`, "/workflows");
}

export async function reviewRejection(
  id: number,
  stepId: number,
  role: "creator" | "assignee",
  _prev: Result | undefined,
  formData: FormData,
) {
  const reason = justification(formData);
  if (typeof reason !== "string") return reason;

  const intent = text(formData, "intent");

  const call =
    role === "creator"
      ? intent === "accept"
        ? workflowService.acceptRejection(id, reason)
        : workflowService.returnToAssignee(id, stepId, reason)
      : intent === "approve"
        ? workflowService.approve(id, reason)
        : workflowService.returnToCreator(id, stepId, reason);

  return mutate(call, `/workflows/${id}`, "/workflows");
}

export async function addAssignee(id: number, _prev: Result | undefined, formData: FormData) {
  const userId = integer(formData, "userId");
  if (!userId) return invalid("Selecione um usuário.");

  return mutate(workflowService.addAssignee(id, userId), `/workflows/${id}`);
}

export async function replaceAssignee(id: number, userId: number, _prev: Result | undefined, formData: FormData) {
  const newUserId = integer(formData, "newUserId");
  if (!newUserId) return invalid("É necessário selecionar um novo responsável para substituir o removido.");

  return mutate(workflowService.replaceAssignee(id, userId, newUserId), `/workflows/${id}`, "/workflows");
}

export async function loadStepMessages(id: number, stepId: number): Promise<Result<WorkflowMessage[]>> {
  const result = await workflowService.messages(id, stepId);
  return result.ok ? ok(result.value) : result;
}
