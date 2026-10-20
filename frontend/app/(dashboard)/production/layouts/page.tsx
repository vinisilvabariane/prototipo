import type { Metadata } from "next";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import DataTable from "@/components/ui/DataTable";
import FilterBar from "@/components/ui/FilterBar";
import FormDialog from "@/components/ui/FormDialog";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import TextLink from "@/components/ui/TextLink";
import { createLayout } from "@/features/production/actions/layoutActions";
import LayoutTargetFields from "@/features/production/editor/LayoutTargetFields";
import { formatDateTime } from "@/features/production/format";
import { productionService } from "@/features/production/services/ProductionService";
import { all } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Layouts de linha",
  description: "Mapas dos postos de cada linha, produto e turno.",
};

const idOf = (value: string | string[] | undefined) => {
  const parsed = Number(typeof value === "string" ? value : "");
  return Number.isInteger(parsed) && parsed > 0 ? parsed : undefined;
};

export default async function LayoutsPage({ searchParams }: PageProps<"/production/layouts">) {
  const params = await searchParams;
  const lineId = idOf(params.lineId);
  const productId = idOf(params.productId);
  const shiftId = idOf(params.shiftId);

  const [layouts, lines, products, shifts] = await Promise.all([
    productionService.layouts({ lineId, productId, shiftId }),
    productionService.lines(),
    productionService.products(),
    productionService.shifts(),
  ]);

  const loaded = all({ layouts, lines, products, shifts });
  if (!loaded.ok) return <PageError title="Layouts de linha" error={loaded.error} retryHref="/production/layouts" />;

  const data = loaded.value;
  const filtered = Boolean(lineId || productId || shiftId);

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Layouts de linha"
        description="O mapa dos postos de cada combinação de linha, produto e turno."
        back={{ href: "/production", label: "Produção" }}
        actions={
          data.layouts.canManage && (
            <FormDialog title="Novo layout" trigger="Novo layout" submitLabel="Criar e abrir" action={createLayout}>
              <LayoutTargetFields lines={data.lines} products={data.products} shifts={data.shifts} />
            </FormDialog>
          )
        }
      />

      <ListCard
        toolbar={
          <FilterBar
            basePath="/production/layouts"
            values={{ lineId: lineId ? String(lineId) : "", productId: productId ? String(productId) : "", shiftId: shiftId ? String(shiftId) : "" }}
            fields={[
              { type: "select", name: "lineId", label: "Linha", options: data.lines.map((line) => ({ value: String(line.id), label: line.name })) },
              { type: "select", name: "productId", label: "Produto", width: 220, options: data.products.map((product) => ({ value: String(product.id), label: `${product.code} · ${product.name}` })) },
              { type: "select", name: "shiftId", label: "Turno", options: data.shifts.map((shift) => ({ value: String(shift.id), label: shift.name })) },
            ]}
          />
        }
      >
        <DataTable
          rows={data.layouts.items}
          rowKey={(layout) => layout.id}
          emptyTitle={filtered ? "Nenhum layout encontrado" : "Nenhum layout criado"}
          emptyHint={filtered ? "Ajuste ou limpe os filtros." : data.layouts.canManage ? "Crie o primeiro em Novo layout." : undefined}
          columns={[
            { key: "line", header: "Linha", strong: true, render: (layout) => <TextLink href={`/production/layouts/${layout.id}`}>{layout.lineName}</TextLink> },
            {
              key: "product",
              header: "Produto",
              render: (layout) => (
                <>
                  {layout.productCode}
                  <Typography component="span" variant="body2" sx={{ color: inkSoft, ml: 1 }}>
                    {layout.productName}
                  </Typography>
                </>
              ),
            },
            { key: "shift", header: "Turno", fit: true, render: (layout) => layout.shiftName },
            { key: "posts", header: "Postos", fit: true, numeric: true, render: (layout) => layout.postCount },
            { key: "updatedAt", header: "Atualizado em", fit: true, numeric: true, align: "left", hideBelow: "md", render: (layout) => formatDateTime(layout.updatedAt) },
          ]}
          actions={(layout) => (
            <Button size="small" href={`/production/layouts/${layout.id}`}>
              {data.layouts.canManage ? "Editar mapa" : "Ver mapa"}
            </Button>
          )}
        />
      </ListCard>
    </Stack>
  );
}
