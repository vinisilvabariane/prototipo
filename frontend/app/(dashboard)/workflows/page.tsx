import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import DataTable from "@/components/ui/DataTable";
import FilterBar from "@/components/ui/FilterBar";
import FormDialog from "@/components/ui/FormDialog";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { createWorkflow } from "@/features/workflows/actions/workflowActions";
import { CategoryChip, DueDate, PriorityChip, StatusChip } from "@/features/workflows/components/WorkflowBadges";
import NewWorkflowFields from "@/features/workflows/components/NewWorkflowFields";
import WorkflowStats from "@/features/workflows/components/WorkflowStats";
import { excerpt, formatDateTime } from "@/features/workflows/format";
import { FILTER_LABELS } from "@/features/workflows/labels";
import { workflowService } from "@/features/workflows/services/WorkflowService";
import type { WorkflowFilter } from "@/features/workflows/types";
import { all } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Workflows",
  description: "Fluxos de aprovação: criação, acompanhamento e decisão dos responsáveis.",
};

const FILTERS = Object.keys(FILTER_LABELS) as WorkflowFilter[];

export default async function WorkflowsPage({ searchParams }: PageProps<"/workflows">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);
  const search = typeof params.search === "string" ? params.search : "";
  const filter = FILTERS.find((item) => item === params.filter) ?? "all";

  const [list, stats, users] = await Promise.all([
    workflowService.list({ page, search, filter }),
    workflowService.stats(),
    workflowService.users(),
  ]);

  const data = all({ list, stats, users });
  if (!data.ok) return <PageError title="Workflows" error={data.error} retryHref="/workflows" />;
  const { list: workflows, stats: totals, users: people } = data.value;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Workflows"
        description="Fluxos de aprovação com responsáveis, justificativas e histórico."
        actions={
          <FormDialog title="Novo workflow" trigger="Novo workflow" submitLabel="Criar workflow" maxWidth="sm" action={createWorkflow}>
            <NewWorkflowFields users={people} />
          </FormDialog>
        }
      />

      <WorkflowStats stats={totals} />

      <ListCard
        toolbar={
          <FilterBar
            basePath="/workflows"
            values={{ search, filter: filter === "all" ? "" : filter }}
            fields={[
              { type: "segment", name: "filter", label: "Filtro de workflows", options: FILTERS.map((item) => ({ value: item === "all" ? "" : item, label: FILTER_LABELS[item] })) },
              { type: "search", name: "search", label: "Buscar por título ou descrição", width: 300 },
            ]}
          />
        }
        footer={<Pager basePath="/workflows" page={workflows.page} pageSize={workflows.pageSize} totalCount={workflows.totalCount} params={{ search, filter: filter === "all" ? undefined : filter }} />}
      >
        <DataTable
          rows={workflows.items}
          rowKey={(workflow) => workflow.id}
          emptyTitle="Nenhum workflow encontrado"
          emptyHint="Crie um novo em Novo workflow ou ajuste os filtros."
          columns={[
            { key: "title", header: "Título", wrap: true, render: (workflow) => <TextLink href={`/workflows/${workflow.id}`}>{workflow.title}</TextLink> },
            { key: "description", header: "Descrição", hideBelow: "lg", wrap: true, render: (workflow) => <span style={{ color: inkSoft }}>{excerpt(workflow.description, 60) || "—"}</span> },
            { key: "priority", header: "Prioridade", fit: true, render: (workflow) => <PriorityChip priority={workflow.priority} /> },
            { key: "category", header: "Categoria", fit: true, hideBelow: "md", render: (workflow) => <CategoryChip category={workflow.category} /> },
            { key: "dueDate", header: "Prazo", fit: true, numeric: true, align: "left", render: (workflow) => <DueDate dueDate={workflow.dueDate} closed={workflow.status !== "inProgress"} /> },
            { key: "createdAt", header: "Criação", fit: true, numeric: true, align: "left", hideBelow: "lg", render: (workflow) => formatDateTime(workflow.createdAt) },
            { key: "status", header: "Status", fit: true, render: (workflow) => <StatusChip status={workflow.status} /> },
          ]}
        />
      </ListCard>
    </Stack>
  );
}
