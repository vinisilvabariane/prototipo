export function formatCnpj(value: string | null | undefined) {
  if (!value) return "—";
  const digits = value.replace(/\D/g, "");
  if (digits.length !== 14) return value;
  return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`;
}

export function formatScore(value: number | null | undefined) {
  return value === null || value === undefined ? "—" : value.toLocaleString("pt-BR", { maximumFractionDigits: 2 });
}

export { formatBytes, formatDateTime, formatDay } from "@/lib/format";
