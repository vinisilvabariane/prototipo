import type { Metadata } from "next";
import { notFound } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import PageError from "@/components/ui/PageError";
import { deleteLayout } from "@/features/production/actions/layoutActions";
import LayoutEditor from "@/features/production/editor/LayoutEditor";
import { formatDateTime } from "@/features/production/format";
import { productionService } from "@/features/production/services/ProductionService";
import StartRunButton from "@/features/production/runs/StartRunButton";
import { all } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";

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
    <Stack spacing={3}>
      <Box>
        <Button href="/production/layouts" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Layouts
        </Button>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ mt: 1, justifyContent: "space-between", alignItems: { md: "flex-start" } }}>
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="overline" component="p" sx={{ color: inkSoft }}>
              {layout.lineName} · {layout.shiftName}
            </Typography>
            <Typography variant="h4" component="h1" sx={{ overflowWrap: "anywhere" }}>
              {layout.productCode} · {layout.productName}
            </Typography>
            <Typography variant="body2" sx={{ color: inkSoft, mt: 0.5 }}>
              Última alteração de {layout.updatedBy || "—"} em {formatDateTime(layout.updatedAt)}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} sx={{ alignItems: "flex-start" }}>
            <StartRunButton layoutId={layout.id} variant="outlined" label="Preparar produção" />
            {layout.canManage && <ConfirmDeleteButton name="este layout" action={deleteLayout.bind(null, layout.id)} />}
          </Stack>
        </Stack>
      </Box>

      <LayoutEditor layout={layout} lines={loaded.value.lines} products={loaded.value.products} shifts={loaded.value.shifts} />
    </Stack>
  );
}
