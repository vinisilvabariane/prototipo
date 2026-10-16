import { apiUrl } from "@/lib/http/env";
import type { AuthTokens } from "./cookies";

export type RefreshOutcome =
  | { status: "renewed"; tokens: AuthTokens }
  | { status: "rejected" }
  | { status: "unavailable" };

const TIMEOUT_MS = 10_000;
const REUSE_WINDOW_MS = 20_000;

const attempts = new Map<string, Promise<RefreshOutcome>>();

function isAuthTokens(value: unknown): value is AuthTokens {
  const data = value as Record<string, unknown> | null;
  return (
    !!data &&
    typeof data.token === "string" &&
    typeof data.refreshToken === "string" &&
    typeof data.refreshExpires === "string"
  );
}

async function callBackend(refreshToken: string): Promise<RefreshOutcome> {
  try {
    const res = await fetch(`${apiUrl()}/api/v1/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
      signal: AbortSignal.timeout(TIMEOUT_MS),
    });

    if (res.ok) {
      const data = await res.json().catch(() => null);
      return isAuthTokens(data) ? { status: "renewed", tokens: data } : { status: "unavailable" };
    }

    return res.status >= 500 || res.status === 429 ? { status: "unavailable" } : { status: "rejected" };
  } catch {
    return { status: "unavailable" };
  }
}

export function renewSession(refreshToken: string): Promise<RefreshOutcome> {
  const existing = attempts.get(refreshToken);
  if (existing) return existing;

  const attempt = callBackend(refreshToken);
  attempts.set(refreshToken, attempt);

  attempt.then((outcome) => {
    const keepFor = outcome.status === "unavailable" ? 0 : REUSE_WINDOW_MS;
    setTimeout(() => attempts.delete(refreshToken), keepFor);
  });

  return attempt;
}
