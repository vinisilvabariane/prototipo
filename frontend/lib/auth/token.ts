export function tokenExpiresInMs(token: string): number | null {
  try {
    const payload = JSON.parse(Buffer.from(token.split(".")[1], "base64url").toString("utf8"));
    return typeof payload.exp === "number" ? payload.exp * 1000 - Date.now() : null;
  } catch {
    return null;
  }
}
