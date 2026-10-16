export const TOKEN_COOKIE = "token";
export const REFRESH_COOKIE = "refreshToken";

export type AuthTokens = { token: string; refreshToken: string; refreshExpires: string };

type CookieOptions = {
  httpOnly: true;
  sameSite: "lax";
  secure: boolean;
  path: string;
  expires?: Date;
};

export type SessionCookie = { name: string; value: string; options: CookieOptions };

export function sessionCookies(auth: AuthTokens): SessionCookie[] {
  const base: CookieOptions = {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
  };

  return [
    { name: TOKEN_COOKIE, value: auth.token, options: base },
    { name: REFRESH_COOKIE, value: auth.refreshToken, options: { ...base, expires: new Date(auth.refreshExpires) } },
  ];
}
