export { formatDateTime } from "@/lib/format";
export { formatHour, formatQuantity } from "@/features/production/format";

export function formatOee(value: number | null | undefined) {
  return value === null || value === undefined ? "—" : `${value.toLocaleString("pt-BR", { maximumFractionDigits: 1 })}%`;
}

export function formatDuration(seconds: number) {
  const total = Math.max(0, Math.round(seconds / 60));
  const hours = Math.floor(total / 60);
  const minutes = total % 60;
  if (hours === 0) return `${minutes} min`;
  return `${hours} h ${String(minutes).padStart(2, "0")}`;
}

export type OeeBand = "world" | "good" | "low" | "poor";

export const BAND_LABELS: Record<OeeBand, string> = {
  world: "Classe mundial",
  good: "Boa",
  low: "Abaixo do esperado",
  poor: "Crítica",
};

export const BAND_COLORS: Record<OeeBand, string> = {
  world: "var(--mui-palette-success-main)",
  good: "var(--mui-palette-primary-main)",
  low: "var(--mui-palette-warning-main)",
  poor: "var(--mui-palette-error-main)",
};

export const bandColor = (value: number) => BAND_COLORS[bandOf(value)];

export function bandOf(value: number): OeeBand {
  if (value >= 85) return "world";
  if (value >= 60) return "good";
  if (value >= 40) return "low";
  return "poor";
}

export const PILLARS = [
  { key: "availability", label: "Disponibilidade", short: "Disp." },
  { key: "performance", label: "Desempenho", short: "Desemp." },
  { key: "quality", label: "Qualidade", short: "Qual." },
] as const;

export function toInputDate(iso: string) {
  return iso.slice(0, 10);
}
