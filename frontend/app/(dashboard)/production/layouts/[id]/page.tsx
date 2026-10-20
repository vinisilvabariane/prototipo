import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Stack from "@mui/material/Stack";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import { deleteLayout } from "@/features/production/actions/layoutActions";
import LayoutEditor from "@/features/production/editor/LayoutEditor";
import { formatDateTime } from "@/features/production/format";
import { productionService } from "@/features/production/services/ProductionService";
import StartRunButton from "@/features/production/runs/StartRunButton";
import { all } from "@/lib/result";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/production/layouts/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Layout não encontrado" };

  const result = await productionService.layout(id);
  if (result.ok) return { title: `${result.value.lineName} · ${result.value.productCode} · ${result.value.shiftName}` };
  return { title: result.error.code === "notFound" ? "Layout não encontrado" : "Layout de linha" };
}

export default async function LayoutPage({ params }: PageProps<"/production/layouts/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const layoutResult = await productionService.layout(id);
  if (!layoutResult.ok && layoutResult.error.code === "notFound") notFound();

  const [lines, products, shifts] = await Promise.all([productionService.lines(), productionService.products(), productionService.shifts()]);
  const loaded = all({ layout: layoutResult, lines, products, shifts });
  if (!loaded.ok) return <PageError title="Layout de linha" error={loaded.error} retryHref={`/production/layouts/${id}`} />;

  const { layout } = loaded.value;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        back={{ href: "/production/layouts", label: "Layouts" }}
        eyebrow={`${layout.lineName} · ${layout.shiftName}`}
        title={`${layout.productCode} · ${layout.productName}`}
        description={`Última alteração de ${layout.updatedBy || "—"} em ${formatDateTime(layout.updatedAt)}`}
        actions={
          <>
            <StartRunButton layoutId={layout.id} variant="outlined" label="Preparar produção" />
            {layout.canManage && <ConfirmDeleteButton name="este layout" action={deleteLayout.bind(null, layout.id)} />}
          </>
        }
      />

      <LayoutEditor layout={layout} lines={loaded.value.lines} products={loaded.value.products} shifts={loaded.value.shifts} />
    </Stack>
  );
}
