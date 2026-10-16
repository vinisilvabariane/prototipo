import type { Metadata } from "next";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
import { saveLine, saveProduct, saveShift } from "@/features/production/actions/setupActions";
import { formatClock } from "@/features/production/format";
import { productionService } from "@/features/production/services/ProductionService";
import { LineFields, ProductFields, ShiftFields } from "@/features/production/setup/SetupFields";
import { all } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Cadastros de produção",
  description: "Linhas, turnos e produtos usados nos layouts e nas produções.",
};

function StatusChip({ active }: { active: boolean }) {
  return <Chip size="small" variant={active ? "filled" : "outlined"} color={active ? "primary" : "default"} label={active ? "Ativo" : "Inativo"} />;
}

function Block({ title, add, children }: { title: string; add: React.ReactNode; children: React.ReactNode }) {
  return (
    <Card sx={{ overflow: "hidden" }}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center", px: 2.5, pt: 2, pb: 1 }}>
        <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
          {title}
        </Typography>
        {add}
      </Stack>
      <Box sx={{ overflowX: "auto" }}>{children}</Box>
    </Card>
  );
}

export default async function ProductionSetupPage() {
  const [lines, shifts, products] = await Promise.all([productionService.lines(true), productionService.shifts(true), productionService.products(true)]);
  const loaded = all({ lines, shifts, products });
  if (!loaded.ok) return <PageError title="Cadastros de produção" error={loaded.error} retryHref="/production/setup" />;

  const data = loaded.value;

  return (
    <Stack spacing={3}>
      <Box>
        <Button href="/production" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Produção
        </Button>
        <Typography variant="h4" component="h1" sx={{ mt: 1 }}>
          Cadastros de produção
        </Typography>
      </Box>

      <Block
        title="Linhas"
        add={
          <FormDialog compact variant="outlined" title="Nova linha" trigger="Adicionar" action={saveLine.bind(null, null)}>
            <LineFields />
          </FormDialog>
        }
      >
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell>Situação</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.lines.length === 0 && (
              <TableRow>
                <TableCell colSpan={3} sx={{ color: inkSoft, textAlign: "center", py: 3 }}>
                  Nenhuma linha cadastrada.
                </TableCell>
              </TableRow>
            )}
            {data.lines.map((line) => (
              <TableRow key={line.id}>
                <TableCell sx={{ fontWeight: 600 }}>{line.name}</TableCell>
                <TableCell>
                  <StatusChip active={line.active} />
                </TableCell>
                <TableCell align="right">
                  <FormDialog compact title="Editar linha" trigger="Editar" action={saveLine.bind(null, line.id)}>
                    <LineFields line={line} />
                  </FormDialog>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Block>

      <Block
        title="Turnos"
        add={
          <FormDialog compact variant="outlined" title="Novo turno" trigger="Adicionar" action={saveShift.bind(null, null)}>
            <ShiftFields />
          </FormDialog>
        }
      >
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell>Horário</TableCell>
              <TableCell>Situação</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.shifts.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} sx={{ color: inkSoft, textAlign: "center", py: 3 }}>
                  Nenhum turno cadastrado.
                </TableCell>
              </TableRow>
            )}
            {data.shifts.map((shift) => (
              <TableRow key={shift.id}>
                <TableCell sx={{ fontWeight: 600 }}>{shift.name}</TableCell>
                <TableCell sx={{ fontVariantNumeric: "tabular-nums" }}>
                  {formatClock(shift.startTime)} às {formatClock(shift.endTime)}
                </TableCell>
                <TableCell>
                  <StatusChip active={shift.active} />
                </TableCell>
                <TableCell align="right">
                  <FormDialog compact title="Editar turno" trigger="Editar" action={saveShift.bind(null, shift.id)}>
                    <ShiftFields shift={shift} />
                  </FormDialog>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Block>

      <Block
        title="Produtos"
        add={
          <FormDialog compact variant="outlined" title="Novo produto" trigger="Adicionar" action={saveProduct.bind(null, null)}>
            <ProductFields />
          </FormDialog>
        }
      >
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Código</TableCell>
              <TableCell>Nome</TableCell>
              <TableCell>Situação</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data.products.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} sx={{ color: inkSoft, textAlign: "center", py: 3 }}>
                  Nenhum produto cadastrado.
                </TableCell>
              </TableRow>
            )}
            {data.products.map((product) => (
              <TableRow key={product.id}>
                <TableCell sx={{ fontWeight: 600, fontVariantNumeric: "tabular-nums" }}>{product.code}</TableCell>
                <TableCell>{product.name}</TableCell>
                <TableCell>
                  <StatusChip active={product.active} />
                </TableCell>
                <TableCell align="right">
                  <FormDialog compact title="Editar produto" trigger="Editar" action={saveProduct.bind(null, product.id)}>
                    <ProductFields product={product} />
                  </FormDialog>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Block>
    </Stack>
  );
}
