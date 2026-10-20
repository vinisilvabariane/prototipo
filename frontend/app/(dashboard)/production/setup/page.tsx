import type { Metadata } from "next";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import DataTable from "@/components/ui/DataTable";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Panel from "@/components/ui/Panel";
import { saveLine, saveProduct, saveShift } from "@/features/production/actions/setupActions";
import { formatClock } from "@/features/production/format";
import { productionService } from "@/features/production/services/ProductionService";
import { LineFields, ProductFields, ShiftFields } from "@/features/production/setup/SetupFields";
import { all } from "@/lib/result";

export const metadata: Metadata = {
  title: "Cadastros de produção",
  description: "Linhas, turnos e produtos usados nos layouts e nas produções.",
};

function StatusChip({ active }: { active: boolean }) {
  return <Chip size="small" variant={active ? "filled" : "outlined"} color={active ? "primary" : "default"} label={active ? "Ativo" : "Inativo"} />;
}

export default async function ProductionSetupPage() {
  const [lines, shifts, products] = await Promise.all([productionService.lines(true), productionService.shifts(true), productionService.products(true)]);
  const loaded = all({ lines, shifts, products });
  if (!loaded.ok) return <PageError title="Cadastros de produção" error={loaded.error} retryHref="/production/setup" />;

  const data = loaded.value;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Cadastros de produção"
        description="Linhas, turnos e produtos que os layouts e as produções usam. Desative em vez de excluir: o histórico continua apontando para eles."
        back={{ href: "/production", label: "Produção" }}
      />

      <Panel
        title="Linhas"
        count={data.lines.length}
        flush
        action={
          <FormDialog compact variant="outlined" title="Nova linha" trigger="Adicionar" action={saveLine.bind(null, null)}>
            <LineFields />
          </FormDialog>
        }
      >
        <DataTable
          rows={data.lines}
          rowKey={(line) => line.id}
          emptyTitle="Nenhuma linha cadastrada"
          columns={[
            { key: "name", header: "Nome", strong: true, render: (line) => line.name },
            { key: "active", header: "Situação", fit: true, render: (line) => <StatusChip active={line.active} /> },
          ]}
          actions={(line) => (
            <FormDialog iconOnly title="Editar linha" trigger="Editar" action={saveLine.bind(null, line.id)}>
              <LineFields line={line} />
            </FormDialog>
          )}
        />
      </Panel>

      <Panel
        title="Turnos"
        count={data.shifts.length}
        flush
        action={
          <FormDialog compact variant="outlined" title="Novo turno" trigger="Adicionar" action={saveShift.bind(null, null)}>
            <ShiftFields />
          </FormDialog>
        }
      >
        <DataTable
          rows={data.shifts}
          rowKey={(shift) => shift.id}
          emptyTitle="Nenhum turno cadastrado"
          columns={[
            { key: "name", header: "Nome", strong: true, render: (shift) => shift.name },
            { key: "time", header: "Horário", fit: true, numeric: true, align: "left", render: (shift) => `${formatClock(shift.startTime)} às ${formatClock(shift.endTime)}` },
            { key: "active", header: "Situação", fit: true, render: (shift) => <StatusChip active={shift.active} /> },
          ]}
          actions={(shift) => (
            <FormDialog iconOnly title="Editar turno" trigger="Editar" action={saveShift.bind(null, shift.id)}>
              <ShiftFields shift={shift} />
            </FormDialog>
          )}
        />
      </Panel>

      <Panel
        title="Produtos"
        count={data.products.length}
        flush
        action={
          <FormDialog compact variant="outlined" title="Novo produto" trigger="Adicionar" action={saveProduct.bind(null, null)}>
            <ProductFields />
          </FormDialog>
        }
      >
        <DataTable
          rows={data.products}
          rowKey={(product) => product.id}
          emptyTitle="Nenhum produto cadastrado"
          columns={[
            { key: "code", header: "Código", fit: true, strong: true, numeric: true, align: "left", render: (product) => product.code },
            { key: "name", header: "Nome", render: (product) => product.name },
            { key: "active", header: "Situação", fit: true, render: (product) => <StatusChip active={product.active} /> },
          ]}
          actions={(product) => (
            <FormDialog iconOnly title="Editar produto" trigger="Editar" action={saveProduct.bind(null, product.id)}>
              <ProductFields product={product} />
            </FormDialog>
          )}
        />
      </Panel>
    </Stack>
  );
}
