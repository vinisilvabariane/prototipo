import "server-only";
import { cookies } from "next/headers";
import type { SessionUser } from "@/features/auth/types";

const TOKEN = "token";
const REFRESH_TOKEN = "refreshToken";

export async function getToken() {
  return (await cookies()).get(TOKEN)?.value;
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
    return {
      name: String(payload.name ?? ""),
      registration: String(payload.registration ?? ""),
      department: String(payload.department ?? ""),
      roles: toList(payload.roles),
    };
  } catch {
    return null;
  }
}

export async function setSession(auth: { token: string; refreshToken: string; refreshExpires: string }) {
  const jar = await cookies();
  const base = {
    httpOnly: true,
    sameSite: "lax" as const,
    secure: process.env.NODE_ENV === "production",
    path: "/",
  };
  jar.set(TOKEN, auth.token, base);
  jar.set(REFRESH_TOKEN, auth.refreshToken, { ...base, expires: new Date(auth.refreshExpires) });
}

export async function clearSession() {
  const jar = await cookies();
  jar.delete(TOKEN);
  jar.delete(REFRESH_TOKEN);
}
