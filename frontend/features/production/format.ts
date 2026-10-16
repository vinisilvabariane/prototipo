export function formatMinutes(minutes: number) {
  if (!minutes || minutes < 1) return "—";
  const total = Math.round(minutes);
  const hours = Math.floor(total / 60);
  const rest = total % 60;
  return hours === 0 ? `${rest} min` : `${hours} h ${String(rest).padStart(2, "0")}`;
}

export function formatPercent(value: number | null | undefined) {
  return value === null || value === undefined ? "—" : `${Math.round(value * 100)}%`;
}

export function formatQuantity(value: number | null | undefined) {
  return value === null || value === undefined ? "—" : value.toLocaleString("pt-BR", { maximumFractionDigits: 1 });
}

export function formatClock(time: string) {
  return time.slice(0, 5);
}

const hour = new Intl.DateTimeFormat("pt-BR", { timeZone: "America/Sao_Paulo", hour: "2-digit", minute: "2-digit" });

export function formatHour(iso: string | null | undefined) {
  return iso ? hour.format(new Date(iso)) : "—";
}

export { formatDateTime } from "@/lib/format";
