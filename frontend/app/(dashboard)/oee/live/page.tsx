import type { Metadata } from "next";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import EquationStrip from "@/features/oee/components/EquationStrip";
import LossNotice from "@/features/oee/components/LossNotice";
import LossWaterfall from "@/features/oee/components/LossWaterfall";
import { formatDuration, formatHour, formatOee } from "@/features/oee/format";
import { oeeService } from "@/features/oee/services/OeeService";
import type { RunOee } from "@/features/oee/types";
import AutoRefresh from "@/features/production/runs/AutoRefresh";
import { all } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "OEE · Tempo real",
  description: "OEE das linhas que estão produzindo agora, atualizado sozinho.",
};

function LiveCard({ run }: { run: RunOee }) {
  const measured = run.posts.filter((post) => post.measured);
  const weakest = [...measured].sort((a, b) => a.pillars.oee - b.pillars.oee)[0];
  const lastHour = run.hours.filter((hour) => hour.produced > 0).at(-1);

  return (
    <Card sx={{ p: 2.5, display: "flex", flexDirection: "column", gap: 2 }}>
      <Box>
        <Typography variant="overline" component="p" sx={{ color: inkSoft }}>
          {run.lineName} · {run.shiftName} · {formatDuration(run.elapsedSeconds)} no ar
        </Typography>
        <Typography variant="h6" component="h2" sx={{ overflowWrap: "anywhere" }}>
          {run.productCode} · {run.productName}
        </Typography>
      </Box>

      <EquationStrip pillars={run.line} compact />
      <LossWaterfall pillars={run.line} />

      <Stack spacing={0.25}>
        {lastHour && (
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Última hora com apontamento ({formatHour(lastHour.hourStart)}): <strong>{formatOee(lastHour.pillars.oee)}</strong>
          </Typography>
        )}
        {weakest && measured.length > 1 && (
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Posto mais baixo: <strong>{weakest.name}</strong> com {formatOee(weakest.pillars.oee)}
          </Typography>
        )}
      </Stack>

      <Button variant="outlined" href={`/oee/runs/${run.runId}`} sx={{ mt: "auto" }}>
        Ver detalhes
      </Button>
    </Card>
  );
}

export default async function OeeLivePage() {
  const list = await oeeService.runs({ status: "running" });
  if (!list.ok) return <PageError title="OEE em tempo real" error={list.error} retryHref="/oee/live" />;

  const details = await Promise.all(list.value.runs.map((run) => oeeService.run(run.runId)));
  const loaded = all(Object.fromEntries(details.map((detail, index) => [String(index), detail])));
  if (!loaded.ok) return <PageError title="OEE em tempo real" error={loaded.error} retryHref="/oee/live" />;

  const runs = Object.values(loaded.value) as RunOee[];
  const readAt = formatHour(new Date().toISOString());

  return (
    <Stack spacing={2.5}>
      <AutoRefresh />

      <PageHeader title="OEE em tempo real" description={`Linhas produzindo agora. A tela se atualiza sozinha a cada 30 segundos · última leitura às ${readAt}.`} />

      <LossNotice available={list.value.lossSourceAvailable} />

      {runs.length === 0 ? (
        <Card>
          <EmptyState title="Nenhuma linha produzindo agora" hint="Quando uma produção for iniciada, o OEE dela aparece aqui." action={<Button href="/production">Ir para produção</Button>} />
        </Card>
      ) : (
        <Box sx={{ display: "grid", gap: 2, gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "repeat(2, minmax(0, 1fr))" } }}>
          {runs.map((run) => (
            <LiveCard key={run.runId} run={run} />
          ))}
        </Box>
      )}
    </Stack>
  );
}
