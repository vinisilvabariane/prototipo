const ZONE = "America/Sao_Paulo";

const isoDay = new Intl.DateTimeFormat("en-CA", { timeZone: ZONE, year: "numeric", month: "2-digit", day: "2-digit" });

export function formatDueDate(value: string | null | undefined) {
  if (!value) return "Não definido";
  const [year, month, day] = value.split("-");
  return `${day}/${month}/${year}`;
}

export function todayInZone() {
  return isoDay.format(new Date());
}

export type DueTone = "overdue" | "today" | "upcoming" | "none";

export function dueTone(dueDate: string | null, closed: boolean): DueTone {
  if (!dueDate || closed) return "none";
  const today = todayInZone();
  if (dueDate < today) return "overdue";
  return dueDate === today ? "today" : "upcoming";
}

export { excerpt, formatBytes, formatDateTime } from "@/lib/format";
