import type { Metadata } from "next";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { instructionService } from "@/features/instructions/services/InstructionService";
import { formatDateTime } from "@/lib/format";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Instruções de trabalho",
  description: "Consulta e cadastro das instruções de trabalho com PDF e passos ilustrados.",
};

export default async function InstructionsPage({ searchParams }: PageProps<"/instructions">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);
  const search = typeof params.search === "string" ? params.search : "";

  const result = await instructionService.list({ page, search });
  if (!result.ok) return <PageError title="Instruções de trabalho" error={result.error} retryHref="/instructions" />;
  const list = result.value;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Instruções de trabalho
        </Typography>
        {list.canManage && (
          <Button href="/instructions/new" variant="contained">
            Nova instrução
          </Button>
        )}
      </Stack>

      <Box component="form" sx={{ display: "flex", gap: 1 }}>
        <TextField name="search" label="Buscar por título, código, produto ou documento" size="small" defaultValue={search} sx={{ minWidth: { sm: 380 } }} />
        <Button type="submit" variant="outlined">
          Buscar
        </Button>
      </Box>

      <Card variant="outlined">
        <Box sx={{ overflowX: "auto" }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Código</TableCell>
                <TableCell>Título</TableCell>
                <TableCell>Produto</TableCell>
                <TableCell>Documento</TableCell>
                <TableCell align="right">Passos</TableCell>
                <TableCell>Atualizada em</TableCell>
                <TableCell align="right">Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {list.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7} sx={{ color: inkSoft, textAlign: "center", py: 4 }}>
                    Nenhuma instrução encontrada.
                  </TableCell>
                </TableRow>
              )}
              {list.items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{item.code}</TableCell>
                  <TableCell sx={{ maxWidth: 320 }}>
                    <TextLink href={`/instructions/${item.id}`}>{item.title}</TextLink>
                  </TableCell>
                  <TableCell>{item.product}</TableCell>
                  <TableCell>{item.documentNumber}</TableCell>
                  <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                    {item.stepCount}
                  </TableCell>
                  <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{formatDateTime(item.updatedAt)}</TableCell>
                  <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                    <Button size="small" href={`/instructions/${item.id}/pdf`} target="_blank" rel="noopener">
                      Abrir PDF
                    </Button>
                    <Button size="small" href={`/instructions/${item.id}`}>
                      Detalhes
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Box>
        <Pager basePath="/instructions" page={list.page} pageSize={list.pageSize} totalCount={list.totalCount} params={{ search }} />
      </Card>
    </Stack>
  );
}
