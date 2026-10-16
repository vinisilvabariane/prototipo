import type { Metadata } from "next";
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
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { createRunFromForm } from "@/features/production/actions/runActions";
import { formatDateTime, formatQuantity } from "@/features/production/format";
import RunStatusChip from "@/features/production/runs/RunStatusChip";
import { productionService } from "@/features/production/services/ProductionService";
import type { RunSummary } from "@/features/production/types";
import { all } from "@/lib/result";
import { brand, inkSoft, line } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Produção",
  description: "Produções em andamento, preparo e encerradas, com alocação de pessoas e produtividade por posto.",
};

function ActiveCard({ run }: { run: RunSummary }) {
  const coverage = run.totalPosts === 0 ? 0 : Math.round((run.allocatedPosts / run.totalPosts) * 100);

  return (
    <Card sx={{ p: 2.5, display: "flex", flexDirection: "column", gap: 1.5 }}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start", gap: 1 }}>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="overline" component="p" sx={{ color: inkSoft }}>
            {run.lineName} · {run.shiftName}
          </Typography>
          <Typography variant="h6" component="h3" sx={{ overflowWrap: "anywhere" }}>
            {run.productCode} · {run.productName}
          </Typography>
        </Box>
        <RunStatusChip status={run.status} />
      </Stack>
      <Box>
        <Stack direction="row" sx={{ justifyContent: "space-between", mb: 0.5 }}>
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Postos ocupados
          </Typography>
          <Typography variant="body2" sx={{ fontWeight: 600, fontVariantNumeric: "tabular-nums" }}>
            {run.allocatedPosts} de {run.totalPosts}
          </Typography>
        </Stack>
        <Box sx={{ height: 6, borderRadius: 3, bgcolor: line, overflow: "hidden" }}>
          <Box sx={{ height: "100%", width: `${coverage}%`, bgcolor: brand }} />
        </Box>
      </Box>
      {run.status === "running" && (
        <Typography variant="body2" sx={{ color: inkSoft }}>
          Produzido <strong>{formatQuantity(run.produced)}</strong> · iniciada em {formatDateTime(run.startedAt)}
        </Typography>
      )}
      <Button variant={run.status === "running" ? "contained" : "outlined"} href={`/production/runs/${run.id}`} sx={{ mt: "auto" }}>
        {run.status === "running" ? "Acompanhar" : "Continuar preparo"}
      </Button>
    </Card>
  );
}

export default async function ProductionPage({ searchParams }: PageProps<"/production">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);

  const [running, preparing, closed, layouts] = await Promise.all([
    productionService.runs({ status: "running", pageSize: 50 }),
    productionService.runs({ status: "preparing", pageSize: 50 }),
    productionService.runs({ status: "closed", page, pageSize: 10 }),
    productionService.layouts(),
  ]);

  const loaded = all({ running, preparing, closed, layouts });
  if (!loaded.ok) return <PageError title="Produção" error={loaded.error} retryHref="/production" />;

  const data = loaded.value;
  const active = [...data.running.items, ...data.preparing.items];

  return (
    <Stack spacing={4}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ justifyContent: "space-between", alignItems: { sm: "center" } }}>
        <Typography variant="h4" component="h1">
          Produção
        </Typography>
        <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", rowGap: 1 }}>
          <Button variant="outlined" href="/production/layouts">
            Layouts de linha
          </Button>
          <Button variant="outlined" href="/production/setup">
            Cadastros
          </Button>
          {data.closed.canSupervise && (
            <FormDialog title="Nova produção" trigger="Nova produção" submitLabel="Preparar" action={createRunFromForm}>
              <Typography variant="body2" sx={{ color: inkSoft }}>
                Escolha o layout. A produção nasce em preparo: você aloca as pessoas nos postos e só então inicia.
              </Typography>
              <TextField select label="Layout" name="layoutId" defaultValue="" required fullWidth>
                {data.layouts.items.map((layout) => (
                  <MenuItem key={layout.id} value={layout.id}>
                    {layout.lineName} · {layout.productCode} · {layout.shiftName}
                  </MenuItem>
                ))}
              </TextField>
            </FormDialog>
          )}
        </Stack>
      </Stack>

      <Box component="section" aria-labelledby="active-runs">
        <Typography id="active-runs" variant="overline" component="h2" sx={{ color: inkSoft }}>
          Em andamento e em preparo ({active.length})
        </Typography>
        {active.length === 0 ? (
          <Typography sx={{ color: inkSoft, mt: 1.5 }}>Nenhuma linha produzindo agora. Use Nova produção para preparar uma.</Typography>
        ) : (
          <Box sx={{ display: "grid", gap: 2, mt: 1.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))", xl: "repeat(3, minmax(0, 1fr))" } }}>
            {active.map((run) => (
              <ActiveCard key={run.id} run={run} />
            ))}
          </Box>
        )}
      </Box>

      <Box component="section" aria-labelledby="closed-runs">
        <Typography id="closed-runs" variant="overline" component="h2" sx={{ color: inkSoft }}>
          Encerradas
        </Typography>
        <Card variant="outlined" sx={{ mt: 1.5 }}>
          <Box sx={{ overflowX: "auto" }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Linha</TableCell>
                  <TableCell>Produto</TableCell>
                  <TableCell>Turno</TableCell>
                  <TableCell>Início</TableCell>
                  <TableCell>Fim</TableCell>
                  <TableCell align="right">Produzido</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.closed.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} sx={{ color: inkSoft, textAlign: "center", py: 4 }}>
                      Nenhuma produção encerrada ainda.
                    </TableCell>
                  </TableRow>
                )}
                {data.closed.items.map((run) => (
                  <TableRow key={run.id}>
                    <TableCell sx={{ fontWeight: 600 }}>
                      <TextLink href={`/production/runs/${run.id}`}>{run.lineName}</TextLink>
                    </TableCell>
                    <TableCell>{run.productCode}</TableCell>
                    <TableCell>{run.shiftName}</TableCell>
                    <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{formatDateTime(run.startedAt)}</TableCell>
                    <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{formatDateTime(run.endedAt)}</TableCell>
                    <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums", fontWeight: 600 }}>
                      {formatQuantity(run.produced)}
                    </TableCell>
                    <TableCell align="right">
                      <Button size="small" href={`/production/runs/${run.id}`}>
                        Ver resultado
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>
          <Pager basePath="/production" page={data.closed.page} pageSize={data.closed.pageSize} totalCount={data.closed.totalCount} />
        </Card>
      </Box>
    </Stack>
  );
}
