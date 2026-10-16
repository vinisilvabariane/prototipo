"use server";

import { mutate } from "@/lib/actions/mutate";
import { integer, text } from "@/lib/actions/form";
import { failure, type Result } from "@/lib/result";
import { orgChartService } from "../services/OrgChartService";

const PATH = "/org-chart";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

const optionalId = (formData: FormData, key: string) => integer(formData, key) || null;

function positionFields(formData: FormData) {
  const title = text(formData, "title");
  const departmentId = integer(formData, "departmentId");
  const problems: string[] = [];

  if (!title) problems.push("Informe o nome do cargo.");
  if (title.length > 100) problems.push("O nome do cargo não pode passar de 100 caracteres.");
  if (!departmentId) problems.push("Selecione o departamento do cargo.");

  return { title, departmentId, problems };
}

export async function createPosition(parentId: number | null, _prev: Result | undefined, formData: FormData) {
  const { title, departmentId, problems } = positionFields(formData);
  if (problems.length > 0) return invalid(...problems);

  return mutate(orgChartService.create({ title, departmentId, parentId, holderId: optionalId(formData, "holderId") }), PATH);
}

export async function updatePosition(id: number, _prev: Result | undefined, formData: FormData) {
  const { title, departmentId, problems } = positionFields(formData);
  if (problems.length > 0) return invalid(...problems);

  return mutate(orgChartService.update(id, { title, departmentId }), PATH);
}

export async function assignHolder(id: number, _prev: Result | undefined, formData: FormData) {
  const userId = integer(formData, "userId");
  if (!userId) return invalid("Selecione um colaborador.");

  return mutate(orgChartService.assignHolder(id, userId), PATH);
}

export async function vacatePosition(id: number) {
  return mutate(orgChartService.assignHolder(id, null), PATH);
}

export async function movePosition(id: number, _prev: Result | undefined, formData: FormData) {
  return mutate(orgChartService.move(id, optionalId(formData, "parentId")), PATH);
}

export async function deletePosition(id: number) {
  return mutate(orgChartService.remove(id), PATH);
}
