import type { Metadata } from "next";
import { notFound } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import { formatDateTime } from "@/features/production/format";
import AutoRefresh from "@/features/production/runs/AutoRefresh";
import ProductivityPanels from "@/features/production/runs/ProductivityPanels";
import RunActions from "@/features/production/runs/RunActions";
import RunBoard from "@/features/production/runs/RunBoard";
import RunStatusChip from "@/features/production/runs/RunStatusChip";
import { productionService } from "@/features/production/services/ProductionService";
import { inkSoft } from "@/lib/tokens";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/production/runs/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Produção não encontrada" };

  const result = await productionService.run(id);
  if (result.ok) return { title: `${result.value.lineName} · ${result.value.productCode}` };
  return { title: result.error.code === "notFound" ? "Produção não encontrada" : "Produção" };
}

export default async function RunPage({ params }: PageProps<"/production/runs/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const result = await productionService.run(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Produção" error={result.error} retryHref={`/production/runs/${id}`} />;

  const run = result.value;

  return (
    <Stack spacing={3}>
      {run.status === "running" && <AutoRefresh />}

      <Box>
        <Button href="/production" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Produção
        </Button>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ mt: 1, justifyContent: "space-between", alignItems: { md: "flex-start" } }}>
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="overline" component="p" sx={{ color: inkSoft }}>
              {run.lineName} · {run.shiftName}
            </Typography>
            <Stack direction="row" spacing={1.5} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
              <Typography variant="h4" component="h1" sx={{ overflowWrap: "anywhere" }}>
                {run.productCode} · {run.productName}
              </Typography>
              <RunStatusChip status={run.status} />
            </Stack>
            <Typography variant="body2" sx={{ color: inkSoft, mt: 0.5 }}>
              Preparada por {run.createdBy} em {formatDateTime(run.createdAt)}
              {run.startedAt && ` · iniciada por ${run.startedBy ?? "—"} em ${formatDateTime(run.startedAt)}`}
              {run.endedAt && ` · encerrada por ${run.endedBy ?? "—"} em ${formatDateTime(run.endedAt)}`}
            </Typography>
          </Box>
          <RunActions runId={run.id} status={run.status} canSupervise={run.canSupervise} hasAllocations={run.posts.some((post) => post.allocation)} />
        </Stack>
      </Box>

      <RunBoard run={run} />
      <ProductivityPanels run={run} />
    </Stack>
  );
}
