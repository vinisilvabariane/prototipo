import type { Metadata } from "next";
import { notFound } from "next/navigation";
import DownloadOutlined from "@mui/icons-material/DownloadOutlined";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Panel from "@/components/ui/Panel";
import EquationStrip from "@/features/oee/components/EquationStrip";
import HourColumns from "@/features/oee/components/HourColumns";
import LossNotice from "@/features/oee/components/LossNotice";
import LossWaterfall from "@/features/oee/components/LossWaterfall";
import PostHourHeatmap from "@/features/oee/components/PostHourHeatmap";
import PostOeeTable from "@/features/oee/components/PostOeeTable";
import { formatDateTime, formatDuration } from "@/features/oee/format";
import { oeeService } from "@/features/oee/services/OeeService";
import AutoRefresh from "@/features/production/runs/AutoRefresh";
import RunStatusChip from "@/features/production/runs/RunStatusChip";
import { inkSoft } from "@/lib/tokens";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/oee/runs/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Produção não encontrada" };

  const result = await oeeService.run(id);
  if (result.ok) return { title: `OEE · ${result.value.lineName} · ${result.value.productCode}` };
  return { title: result.error.code === "notFound" ? "Produção não encontrada" : "OEE" };
}

export default async function RunOeePage({ params }: PageProps<"/oee/runs/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const result = await oeeService.run(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="OEE" error={result.error} retryHref={`/oee/runs/${id}`} />;

  const run = result.value;
  const started = run.startedAt !== null;

  return (
    <Stack spacing={2.5}>
      {run.status === "running" && <AutoRefresh />}

      <PageHeader
        back={{ href: "/oee", label: "Comparativo" }}
        eyebrow={`${run.lineName} · ${run.shiftName}`}
        title={`${run.productCode} · ${run.productName}`}
        badges={<RunStatusChip status={run.status} />}
        description={
          started
            ? `Iniciada em ${formatDateTime(run.startedAt)}${run.endedAt ? ` e encerrada em ${formatDateTime(run.endedAt)}` : ""} · ${formatDuration(run.elapsedSeconds)} de produção · fórmulas: ${run.formulaSetName}`
            : "A produção ainda está em preparo. O OEE aparece depois que ela for iniciada."
        }
        actions={
          <>
            <Button variant="outlined" href={`/production/runs/${run.runId}`}>
              Ver produção
            </Button>
            {started && (
              <Button variant="outlined" startIcon={<DownloadOutlined />} component="a" href={`/oee/export?runId=${run.runId}`}>
                Exportar planilha
              </Button>
            )}
          </>
        }
      />

      {!started ? (
        <Panel title="Sem dados ainda">
          <EmptyState compact title="Nada para calcular" hint="Inicie a produção na tela de alocação para começar a medir." />
        </Panel>
      ) : (
        <>
          <LossNotice available={run.lossSourceAvailable} />

          <Panel title="OEE da linha">
            <Stack spacing={2.5}>
              <EquationStrip pillars={run.line} />
              <LossWaterfall pillars={run.line} />
              <Typography variant="caption" sx={{ color: inkSoft }}>
                Produzido {run.produced.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} de {run.theoretical.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} peças previstas no último posto do fluxo. Cada perda é
                calculada sobre o que sobrou da etapa anterior.
              </Typography>
            </Stack>
          </Panel>

          <Panel title="Hora a hora" count={run.hours.length}>
            <HourColumns hours={run.hours} />
          </Panel>

          <Panel title="Mapa de calor por posto">
            <PostHourHeatmap posts={run.posts} />
          </Panel>

          <Panel title="Por posto" count={run.posts.length} flush>
            <PostOeeTable posts={run.posts} lastPostId={run.lastPostId} />
          </Panel>
        </>
      )}
    </Stack>
  );
}
