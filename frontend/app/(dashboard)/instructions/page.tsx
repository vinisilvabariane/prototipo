import type { Metadata } from "next";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import DataTable from "@/components/ui/DataTable";
import FilterBar from "@/components/ui/FilterBar";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { instructionService } from "@/features/instructions/services/InstructionService";
import { formatDateTime } from "@/lib/format";

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
    <Stack spacing={2.5}>
      <PageHeader
        title="Instruções de trabalho"
        description="Como cada operação é executada: EPI, especificações, passos ilustrados e PDF."
        actions={
          list.canManage && (
            <Button href="/instructions/new" variant="contained">
              Nova instrução
            </Button>
          )
        }
      />

      <ListCard
        toolbar={<FilterBar basePath="/instructions" values={{ search }} fields={[{ type: "search", name: "search", label: "Buscar por título, código, produto ou documento", width: 420 }]} />}
        footer={<Pager basePath="/instructions" page={list.page} pageSize={list.pageSize} totalCount={list.totalCount} params={{ search }} />}
      >
        <DataTable
          rows={list.items}
          rowKey={(item) => item.id}
          emptyTitle={search ? "Nenhuma instrução encontrada" : "Nenhuma instrução cadastrada"}
          emptyHint={search ? "Tente outro termo de busca." : list.canManage ? "Cadastre a primeira em Nova instrução." : undefined}
          columns={[
            { key: "code", header: "Código", fit: true, numeric: true, align: "left", render: (item) => item.code },
            { key: "title", header: "Título", wrap: true, render: (item) => <TextLink href={`/instructions/${item.id}`}>{item.title}</TextLink> },
            { key: "product", header: "Produto", hideBelow: "md", render: (item) => item.product },
            { key: "documentNumber", header: "Documento", hideBelow: "lg", render: (item) => item.documentNumber },
            { key: "steps", header: "Passos", fit: true, numeric: true, render: (item) => item.stepCount },
            { key: "updatedAt", header: "Atualizada em", fit: true, numeric: true, align: "left", hideBelow: "lg", render: (item) => formatDateTime(item.updatedAt) },
          ]}
          actions={(item) => (
            <Button size="small" href={`/instructions/${item.id}/pdf`} target="_blank" rel="noopener">
              Abrir PDF
            </Button>
          )}
        />
      </ListCard>
    </Stack>
  );
}
