import "server-only";

export function apiUrl() {
  const url = process.env.API_URL;
  if (!url) throw new Error("API_URL não está definida. Crie frontend/.env.local com API_URL=http://localhost:5156");
  return url.replace(/\/+$/, "");
}
