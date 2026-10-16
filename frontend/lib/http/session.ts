import "server-only";
import { cookies } from "next/headers";
import type { SessionUser } from "@/features/auth/types";
import { REFRESH_COOKIE, TOKEN_COOKIE, sessionCookies, type AuthTokens } from "@/lib/auth/cookies";

export async function getToken() {
  return (await cookies()).get(TOKEN_COOKIE)?.value;
}

function toList(value: unknown): string[] {
  if (Array.isArray(value)) return value.filter((item): item is string => typeof item === "string");
  return typeof value === "string" && value ? [value] : [];
}

export async function readSessionUser(): Promise<SessionUser | null> {
  const token = await getToken();
  if (!token) return null;

  try {
    const payload = JSON.parse(Buffer.from(token.split(".")[1], "base64url").toString("utf8"));
    const id = Number(payload.sub);
    return {
      id: Number.isInteger(id) && id > 0 ? id : null,
      name: String(payload.name ?? ""),
      registration: String(payload.registration ?? ""),
      department: String(payload.department ?? ""),
      roles: toList(payload.roles),
    };
  } catch {
    return null;
  }
}

export async function setSession(auth: AuthTokens) {
  const jar = await cookies();
  for (const cookie of sessionCookies(auth)) jar.set(cookie.name, cookie.value, cookie.options);
}

export async function clearSession() {
  const jar = await cookies();
  jar.delete(TOKEN_COOKIE);
  jar.delete(REFRESH_COOKIE);
}
