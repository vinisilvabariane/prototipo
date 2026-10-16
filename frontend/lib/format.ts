const ZONE = "America/Sao_Paulo";

const dateTime = new Intl.DateTimeFormat("pt-BR", {
  timeZone: ZONE,
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

export function formatDateTime(iso: string | null | undefined) {
  return iso ? dateTime.format(new Date(iso)) : "—";
}

export function formatBytes(bytes: number) {
  if (!bytes) return "0 B";
  const units = ["B", "KB", "MB", "GB"];
  const index = Math.min(units.length - 1, Math.floor(Math.log(bytes) / Math.log(1024)));
  const value = bytes / 1024 ** index;
  return `${value.toLocaleString("pt-BR", { maximumFractionDigits: index === 0 ? 0 : 2 })} ${units[index]}`;
}

export function excerpt(text: string, length = 60) {
  return text.length > length ? `${text.slice(0, length).trimEnd()}…` : text;
}

export function formatDay(value: string | null | undefined) {
  if (!value) return "—";
  const [year, month, day] = value.split("-");
  return `${day}/${month}/${year}`;
}
