"use server";

import { mutate } from "@/lib/actions/mutate";
import { integer, secret, text } from "@/lib/actions/form";
import type { Result } from "@/lib/result";
import { userService } from "../services/UserService";

const fields = (formData: FormData) => ({
  registration: integer(formData, "registration"),
  name: text(formData, "name"),
  email: text(formData, "email") || null,
  department: text(formData, "department"),
});

export async function createUser(_prev: Result | undefined, formData: FormData) {
  return mutate(userService.create({ ...fields(formData), password: secret(formData, "password") }), "/users");
}

export async function updateUser(id: number, _prev: Result | undefined, formData: FormData) {
  return mutate(userService.update(id, fields(formData)), "/users");
}

export async function deleteUser(id: number) {
  return mutate(userService.remove(id), "/users");
}
