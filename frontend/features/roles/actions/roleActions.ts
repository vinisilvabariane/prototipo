"use server";

import { mutate } from "@/lib/actions/mutate";
import { text } from "@/lib/actions/form";
import type { Result } from "@/lib/result";
import { roleService } from "../services/RoleService";

const fields = (formData: FormData) => ({
  name: text(formData, "name"),
  description: text(formData, "description"),
});

export async function createRole(_prev: Result | undefined, formData: FormData) {
  return mutate(roleService.create(fields(formData)), "/roles");
}

export async function updateRole(id: number, _prev: Result | undefined, formData: FormData) {
  return mutate(roleService.update(id, fields(formData)), "/roles");
}

export async function deleteRole(id: number) {
  return mutate(roleService.remove(id), "/roles");
}
