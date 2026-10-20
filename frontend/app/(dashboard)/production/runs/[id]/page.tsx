import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import { formatDateTime } from "@/features/production/format";
import AutoRefresh from "@/features/production/runs/AutoRefresh";
import ProductivityPanels from "@/features/production/runs/ProductivityPanels";
import RunActions from "@/features/production/runs/RunActions";
import RunBoard from "@/features/production/runs/RunBoard";
import RunStatusChip from "@/features/production/runs/RunStatusChip";
import { productionService } from "@/features/production/services/ProductionService";

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
    <Stack spacing={2.5}>
      {run.status === "running" && <AutoRefresh />}

      <PageHeader
        back={{ href: "/production", label: "Produção" }}
        eyebrow={`${run.lineName} · ${run.shiftName}`}
        title={`${run.productCode} · ${run.productName}`}
        badges={<RunStatusChip status={run.status} />}
        description={
          <>
            Preparada por {run.createdBy} em {formatDateTime(run.createdAt)}
            {run.startedAt && ` · iniciada por ${run.startedBy ?? "—"} em ${formatDateTime(run.startedAt)}`}
            {run.endedAt && ` · encerrada por ${run.endedBy ?? "—"} em ${formatDateTime(run.endedAt)}`}
          </>
        }
        actions={<RunActions runId={run.id} status={run.status} canSupervise={run.canSupervise} hasAllocations={run.posts.some((post) => post.allocation)} />}
      />

      <RunBoard run={run} />
      <ProductivityPanels run={run} />
    </Stack>
  );
}
