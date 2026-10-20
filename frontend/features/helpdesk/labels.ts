import type { TicketStatus } from "./types";

export const STATUS_LABELS: Record<TicketStatus, string> = {
  open: "Aberto",
  inProgress: "Em atendimento",
  resolved: "Resolvido",
  rejected: "Recusado",
};

export const STATUS_ORDER: TicketStatus[] = ["open", "inProgress", "resolved", "rejected"];

export function formatElapsed(fromIso: string | null | undefined, toIso?: string | null) {
  if (!fromIso) return "—";
  const end = toIso ? new Date(toIso).getTime() : Date.now();
  const minutes = Math.max(0, Math.round((end - new Date(fromIso).getTime()) / 60_000));
  if (minutes < 1) return "agora";
  if (minutes < 60) return `${minutes} min`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} h ${String(minutes % 60).padStart(2, "0")}`;
  return `${Math.floor(hours / 24)} d ${hours % 24} h`;
}

export function formatDuration(seconds: number | null | undefined) {
  if (seconds === null || seconds === undefined) return "—";
  return formatElapsed(new Date(0).toISOString(), new Date(seconds * 1000).toISOString());
}
