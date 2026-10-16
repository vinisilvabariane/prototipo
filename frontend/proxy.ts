import { NextResponse, type NextRequest } from "next/server";
import { REFRESH_COOKIE, TOKEN_COOKIE, sessionCookies } from "@/lib/auth/cookies";
import { renewSession } from "@/lib/auth/refresh";
import { tokenExpiresInMs } from "@/lib/auth/token";

const RENEW_BEFORE_MS = 60_000;

function endSession(request: NextRequest) {
  const response = NextResponse.redirect(new URL("/login", request.url));
  response.cookies.delete(TOKEN_COOKIE);
  response.cookies.delete(REFRESH_COOKIE);
  return response;
}

export async function proxy(request: NextRequest) {
  const token = request.cookies.get(TOKEN_COOKIE)?.value;
  const refreshToken = request.cookies.get(REFRESH_COOKIE)?.value;

  const expiresIn = token ? tokenExpiresInMs(token) : null;
  const tokenIsFresh = Boolean(token) && (expiresIn === null || expiresIn > RENEW_BEFORE_MS);

  if (request.nextUrl.pathname === "/login" || tokenIsFresh) return NextResponse.next();
  if (!refreshToken) return endSession(request);

  const outcome = await renewSession(refreshToken);
  if (outcome.status === "rejected") return endSession(request);
  if (outcome.status === "unavailable") return NextResponse.next();

  const cookies = sessionCookies(outcome.tokens);
  for (const cookie of cookies) request.cookies.set(cookie.name, cookie.value);

  const response = NextResponse.next({ request });
  for (const cookie of cookies) response.cookies.set(cookie.name, cookie.value, cookie.options);
  return response;
}

export const config = {
  matcher: ["/((?!_next|favicon.ico|.*\\..*).*)"],
};
