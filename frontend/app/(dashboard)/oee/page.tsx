import type { Metadata } from "next";
import DownloadOutlined from "@mui/icons-material/DownloadOutlined";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import EmptyState from "@/components/ui/EmptyState";
import FilterBar from "@/components/ui/FilterBar";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Panel from "@/components/ui/Panel";
import TextLink from "@/components/ui/TextLink";
import LossNotice from "@/features/oee/components/LossNotice";
import OeeBar from "@/features/oee/components/OeeBar";
import { formatDateTime, formatDuration, formatOee, toInputDate } from "@/features/oee/format";
import { oeeService } from "@/features/oee/services/OeeService";
import type { OeeRunSummary } from "@/features/oee/types";
import RunStatusChip from "@/features/production/runs/RunStatusChip";
import { productionService } from "@/features/production/services/ProductionService";
import { all } from "@/lib/result";
import { inkSoft, line } from "@/lib/tokens";
import { qs } from "@/lib/http/query";

export const metadata: Metadata = {
  title: "OEE · Comparativo",
  description: "Compare o OEE das linhas e das produções em um período: disponibilidade, desempenho e qualidade.",
};

function Stat({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Box sx={{ px: 2.5, py: 1.5, minWidth: 0 }}>
      <Typography variant="overline" component="p" sx={{ color: inkSoft, lineHeight: 1.4 }}>
        {label}
      </Typography>
      <Typography component="p" sx={{ fontSize: 24, fontWeight: 700, lineHeight: 1.2, fontVariantNumeric: "tabular-nums" }}>
        {value}
      </Typography>
      {hint && (
        <Typography variant="caption" component="p" noWrap sx={{ color: inkSoft }} title={hint}>
          {hint}
        </Typography>
      )}
    </Box>
  );
}

function RankingRow({ run }: { run: OeeRunSummary }) {
  return (
    <Box
      component="li"
      sx={{
        display: "grid",
        alignItems: "center",
        gap: { xs: 0.75, md: 3 },
        px: 2.5,
        py: 1.25,
        borderTop: `1px solid ${line}`,
        gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 1.2fr) minmax(0, 1fr) minmax(0, 1.1fr)" },
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <TextLink href={`/oee/runs/${run.runId}`}>{run.lineName}</TextLink>
          <RunStatusChip status={run.status} />
        </Stack>
        <Typography variant="body2" noWrap sx={{ color: inkSoft }}>
          {run.productCode} · {run.shiftName} · {formatDateTime(run.startedAt)} · {formatDuration(run.elapsedSeconds)}
        </Typography>
      </Box>
      <Box sx={{ minWidth: 0 }}>
        <OeeBar value={run.pillars.oee} width={200} strong />
      </Box>
      <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
        Disp. {formatOee(run.pillars.availability)} · Desemp. {formatOee(run.pillars.performance)} · Qual. {formatOee(run.pillars.quality)}
      </Typography>
    </Box>
  );
}

export default async function OeeComparisonPage({ searchParams }: PageProps<"/oee">) {
  const params = await searchParams;
  const pick = (key: string) => (typeof params[key] === "string" ? (params[key] as string) : "");
  const query = { from: pick("from"), to: pick("to"), lineId: pick("lineId"), status: pick("status") };

  const [comparison, lines] = await Promise.all([oeeService.runs(query), productionService.lines(true)]);
  const loaded = all({ comparison, lines });
  if (!loaded.ok) return <PageError title="OEE" error={loaded.error} retryHref="/oee" />;

  const { comparison: data, lines: lineList } = loaded.value;
  const ranked = [...data.runs].sort((a, b) => b.pillars.oee - a.pillars.oee);
  const average = ranked.length === 0 ? 0 : ranked.reduce((sum, run) => sum + run.pillars.oee, 0) / ranked.length;
  const best = ranked[0];
  const worst = ranked.length > 1 ? ranked.at(-1) : undefined;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Comparativo de OEE"
        description={`Produções iniciadas no período, da melhor para a pior. Fórmulas em uso: ${data.formulaSetName}.`}
        actions={
          <Button variant="outlined" startIcon={<DownloadOutlined />} component="a" href={`/oee/export${qs({ ...query, from: query.from || toInputDate(data.from), to: query.to || toInputDate(data.to) })}`}>
            Exportar planilha
          </Button>
        }
      />

      <FilterBar
        basePath="/oee"
        values={{ from: query.from || toInputDate(data.from), to: query.to || toInputDate(data.to), lineId: query.lineId, status: query.status }}
        fields={[
          { type: "date", name: "from", label: "De" },
          { type: "date", name: "to", label: "Até" },
          { type: "select", name: "lineId", label: "Linha", options: lineList.map((item) => ({ value: String(item.id), label: item.name })) },
          {
            type: "segment",
            name: "status",
            label: "Situação",
            options: [
              { value: "", label: "Todas" },
              { value: "running", label: "Em andamento" },
              { value: "closed", label: "Encerradas" },
            ],
          },
        ]}
      />

      <LossNotice available={data.lossSourceAvailable} />

      <Card sx={{ display: "grid", gridTemplateColumns: { xs: "repeat(2, minmax(0, 1fr))", md: "repeat(4, minmax(0, 1fr))" }, "& > *:not(:first-of-type)": { borderLeft: { md: `1px solid ${line}` } } }}>
        <Stat label="Produções" value={String(ranked.length)} />
        <Stat label="OEE médio" value={ranked.length ? formatOee(Math.round(average * 10) / 10) : "—"} />
        <Stat label="Melhor" value={best ? formatOee(best.pillars.oee) : "—"} hint={best ? `${best.lineName} · ${best.productCode}` : undefined} />
        <Stat label="Pior" value={worst ? formatOee(worst.pillars.oee) : "—"} hint={worst ? `${worst.lineName} · ${worst.productCode}` : undefined} />
      </Card>

      <Panel title="Ranking" count={ranked.length} flush>
        {ranked.length === 0 ? (
          <EmptyState title="Nenhuma produção iniciada neste período" hint="Amplie o período ou limpe os filtros. Só entram produções que já foram iniciadas." />
        ) : (
          <Box component="ul" sx={{ listStyle: "none", m: 0, p: 0 }}>
            {ranked.map((run) => (
              <RankingRow key={run.runId} run={run} />
            ))}
          </Box>
        )}
      </Panel>
    </Stack>
  );
}
