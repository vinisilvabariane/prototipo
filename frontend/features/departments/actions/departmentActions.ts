"use server";

import { mutate } from "@/lib/actions/mutate";
import { text } from "@/lib/actions/form";
import type { Result } from "@/lib/result";
import { departmentService } from "../services/DepartmentService";

export async function createDepartment(_prev: Result | undefined, formData: FormData) {
  return mutate(departmentService.create(text(formData, "name")), "/departments", "/users");
}

export async function updateDepartment(id: number, _prev: Result | undefined, formData: FormData) {
  return mutate(departmentService.update(id, text(formData, "name")), "/departments", "/users");
}

export async function deleteDepartment(id: number) {
  return mutate(departmentService.remove(id), "/departments", "/users");
}
