"use server";

import { redirect } from "next/navigation";
import { integer, secret } from "@/lib/actions/form";
import { clearSession, setSession } from "@/lib/http/session";
import type { Result } from "@/lib/result";
import { authService } from "../services/AuthService";

export async function login(_prev: Result | undefined, formData: FormData): Promise<Result> {
  const result = await authService.login(integer(formData, "registration"), secret(formData, "password"));
  if (!result.ok) return result;

  await setSession(result.value);
  redirect("/hub");
}

export async function logout() {
  await clearSession();
  redirect("/login");
}
