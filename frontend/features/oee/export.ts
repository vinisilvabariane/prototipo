import { RUN_STATUS_LABELS, POST_KIND_LABELS } from "@/features/production/labels";
import { formatDateTime, formatHour } from "./format";
import type { OeeComparison, RunOee } from "./types";

type Cell = string | number | null | undefined;

const BOM = String.fromCharCode(0xfeff);
const FORMULA_START = /^[=+\-@\t\r]/;

function cell(value: Cell) {
  if (value === null || value === undefined) return "";
  if (typeof value === "number") return String(Math.round(value * 100) / 100).replace(".", ",");
  const safe = FORMULA_START.test(value) ? `'${value}` : value;
  return /[;"\r\n]/.test(safe) ? `"${safe.replaceAll('"', '""')}"` : safe;
}

const row = (...cells: Cell[]) => cells.map(cell).join(";");

const csv = (rows: string[]) => `${BOM}${rows.join("\r\n")}\r\n`;

export function fileSafe(text: string) {
  return text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .replace(/[^a-zA-Z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
    .toLowerCase();
}

export function runCsv(run: RunOee) {
  const posts = [...run.posts].sort((a, b) => a.x - b.x || a.postId - b.postId);
  const measured = posts.filter((post) => post.measured && post.hours.length > 0);

  const rows = [
    row("Linha", run.lineName),
    row("Produto", `${run.productCode} · ${run.productName}`),
    row("Turno", run.shiftName),
    row("Situação", RUN_STATUS_LABELS[run.status]),
    row("Início", formatDateTime(run.startedAt)),
    row("Fim", formatDateTime(run.endedAt)),
    row("Duração (min)", Math.round(run.elapsedSeconds / 60)),
    row("Fórmulas", run.formulaSetName),
    row("Paradas e defeitos", run.lossSourceAvailable ? "Incluídos" : "Ainda não incluídos (help desk)"),
    "",
    row("OEE da linha (%)", "Disponibilidade (%)", "Desempenho (%)", "Qualidade (%)", "Produzido", "Previsto"),
    row(run.line.oee, run.line.availability, run.line.performance, run.line.quality, run.produced, run.theoretical),
    "",
    row("Posto", "Tipo", "Meta por hora", "Medido", "Produzido", "Previsto", "Tempo com gente (min)", "Disponibilidade (%)", "Desempenho (%)", "Qualidade (%)", "OEE (%)"),
    ...posts.map((post) =>
      row(
        post.name,
        POST_KIND_LABELS[post.kind],
        post.targetPerHour,
        post.measured ? "Sim" : "Não",
        post.measured ? post.produced : null,
        post.measured ? post.theoretical : null,
        Math.round(post.occupiedSeconds / 60),
        post.measured ? post.pillars.availability : null,
        post.measured ? post.pillars.performance : null,
        post.measured ? post.pillars.quality : null,
        post.measured ? post.pillars.oee : null,
      ),
    ),
    "",
    row("Hora", "OEE da linha (%)", "Disponibilidade (%)", "Desempenho (%)", "Qualidade (%)", "Produzido", "Previsto"),
    ...run.hours.map((hour) => row(formatHour(hour.hourStart), hour.pillars.oee, hour.pillars.availability, hour.pillars.performance, hour.pillars.quality, hour.produced, hour.theoretical)),
  ];

  if (measured.length > 0) {
    rows.push("", row("OEE por posto e hora (%)", ...measured[0].hours.map((hour) => formatHour(hour.hourStart))));
    for (const post of measured) rows.push(row(post.name, ...post.hours.map((hour) => hour.pillars.oee)));
  }

  return csv(rows);
}

export function comparisonCsv(data: OeeComparison) {
  return csv([
    row("Fórmulas", data.formulaSetName),
    row("Paradas e defeitos", data.lossSourceAvailable ? "Incluídos" : "Ainda não incluídos (help desk)"),
    "",
    row("Linha", "Produto", "Turno", "Situação", "Início", "Fim", "Duração (min)", "Disponibilidade (%)", "Desempenho (%)", "Qualidade (%)", "OEE (%)", "Produzido", "Previsto", "Postos medidos", "Postos"),
    ...data.runs.map((run) =>
      row(
        run.lineName,
        `${run.productCode} · ${run.productName}`,
        run.shiftName,
        RUN_STATUS_LABELS[run.status],
        formatDateTime(run.startedAt),
        formatDateTime(run.endedAt),
        Math.round(run.elapsedSeconds / 60),
        run.pillars.availability,
        run.pillars.performance,
        run.pillars.quality,
        run.pillars.oee,
        run.produced,
        run.theoretical,
        run.measuredPosts,
        run.totalPosts,
      ),
    ),
  ]);
}
