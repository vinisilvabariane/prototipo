import type { Metadata } from "next";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
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

  return (
    <Stack spacing={3}>
      <Box>
        <Button href="/production" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Produção
        </Button>
        <Stack direction="row" sx={{ mt: 1, justifyContent: "space-between", alignItems: "center" }}>
          <Typography variant="h4" component="h1">
            Layouts de linha
          </Typography>
          {data.layouts.canManage && (
            <FormDialog title="Novo layout" trigger="Novo layout" submitLabel="Criar e abrir" action={createLayout}>
              <LayoutTargetFields lines={data.lines} products={data.products} shifts={data.shifts} />
            </FormDialog>
          )}
        </Stack>
      </Box>

      <Box component="form" action="/production/layouts" sx={{ display: "flex", gap: 1, flexWrap: "wrap", alignItems: "center" }}>
        {[
          { name: "lineId", label: "Linha", value: lineId, options: data.lines.map((line) => [line.id, line.name] as const) },
          { name: "productId", label: "Produto", value: productId, options: data.products.map((product) => [product.id, `${product.code} · ${product.name}`] as const) },
          { name: "shiftId", label: "Turno", value: shiftId, options: data.shifts.map((shift) => [shift.id, shift.name] as const) },
        ].map((filter) => (
          <TextField key={filter.name} select size="small" name={filter.name} label={filter.label} defaultValue={filter.value ?? ""} sx={{ minWidth: 190 }}>
            <MenuItem value="">Todos</MenuItem>
            {filter.options.map(([id, label]) => (
              <MenuItem key={id} value={id}>
                {label}
              </MenuItem>
            ))}
          </TextField>
        ))}
        <Button type="submit" variant="outlined">
          Filtrar
        </Button>
        <Button href="/production/layouts">Limpar</Button>
      </Box>

      <Card variant="outlined">
        <Box sx={{ overflowX: "auto" }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Linha</TableCell>
                <TableCell>Produto</TableCell>
                <TableCell>Turno</TableCell>
                <TableCell align="right">Postos</TableCell>
                <TableCell>Atualizado em</TableCell>
                <TableCell align="right">Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.layouts.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} sx={{ color: inkSoft, textAlign: "center", py: 4 }}>
                    Nenhum layout encontrado. {data.layouts.canManage ? "Crie o primeiro em Novo layout." : ""}
                  </TableCell>
                </TableRow>
              )}
              {data.layouts.items.map((layout) => (
                <TableRow key={layout.id}>
                  <TableCell sx={{ fontWeight: 600 }}>
                    <TextLink href={`/production/layouts/${layout.id}`}>{layout.lineName}</TextLink>
                  </TableCell>
                  <TableCell>
                    {layout.productCode}
                    <Typography component="span" variant="body2" sx={{ color: inkSoft, ml: 1 }}>
                      {layout.productName}
                    </Typography>
                  </TableCell>
                  <TableCell>{layout.shiftName}</TableCell>
                  <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                    {layout.postCount}
                  </TableCell>
                  <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{formatDateTime(layout.updatedAt)}</TableCell>
                  <TableCell align="right">
                    <Button size="small" href={`/production/layouts/${layout.id}`}>
                      {data.layouts.canManage ? "Editar mapa" : "Ver mapa"}
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Box>
      </Card>
    </Stack>
  );
}
