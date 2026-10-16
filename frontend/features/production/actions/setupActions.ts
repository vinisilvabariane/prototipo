"use server";

import { text } from "@/lib/actions/form";
import { mutate } from "@/lib/actions/mutate";
import { failure, type Result } from "@/lib/result";
import { FIELD_LIMITS } from "../labels";
import { productionService } from "../services/ProductionService";

const invalid = (...messages: string[]) => failure("badRequest", { messages });

const REFRESH = ["/production/setup", "/production/layouts", "/production"];

function checkName(value: string, label: string, max: number, problems: string[]) {
  if (!value) problems.push(`Informe ${label}.`);
  else if (value.length > max) problems.push(`O campo ${label} não pode passar de ${max} caracteres.`);
}

function timeOf(formData: FormData, key: string, label: string, problems: string[]) {
  const value = text(formData, key);
  if (!/^\d{2}:\d{2}$/.test(value)) {
    problems.push(`Informe ${label}.`);
    return "";
  }
  return `${value}:00`;
}

export async function saveLine(id: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const name = text(formData, "name");
  checkName(name, "o nome da linha", FIELD_LIMITS.lineName, problems);
  if (problems.length > 0) return invalid(...problems);

  const body = { name, active: formData.get("active") === "on" };
  return mutate(id === null ? productionService.createSetup("lines", body) : productionService.updateSetup("lines", id, body), ...REFRESH);
}

export async function saveShift(id: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const name = text(formData, "name");
  checkName(name, "o nome do turno", FIELD_LIMITS.shiftName, problems);
  const startTime = timeOf(formData, "startTime", "o início do turno", problems);
  const endTime = timeOf(formData, "endTime", "o fim do turno", problems);
  if (startTime && startTime === endTime) problems.push("O início e o fim do turno não podem ser iguais.");
  if (problems.length > 0) return invalid(...problems);

  const body = { name, startTime, endTime, active: formData.get("active") === "on" };
  return mutate(id === null ? productionService.createSetup("shifts", body) : productionService.updateSetup("shifts", id, body), ...REFRESH);
}

export async function saveProduct(id: number | null, _prev: Result | undefined, formData: FormData) {
  const problems: string[] = [];
  const code = text(formData, "code");
  const name = text(formData, "name");
  checkName(code, "o código do produto", FIELD_LIMITS.productCode, problems);
  checkName(name, "o nome do produto", FIELD_LIMITS.productName, problems);
  if (problems.length > 0) return invalid(...problems);

  const body = { code, name, active: formData.get("active") === "on" };
  return mutate(id === null ? productionService.createSetup("products", body) : productionService.updateSetup("products", id, body), ...REFRESH);
}
