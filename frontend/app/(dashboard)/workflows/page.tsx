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
import Typography from "@mui/material/Typography";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { createWorkflow } from "@/features/workflows/actions/workflowActions";
import { CategoryChip, DueDate, PriorityChip, StatusChip } from "@/features/workflows/components/WorkflowBadges";
import NewWorkflowFields from "@/features/workflows/components/NewWorkflowFields";
import WorkflowFilters from "@/features/workflows/components/WorkflowFilters";
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
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Workflows
        </Typography>
        <FormDialog title="Novo workflow" trigger="Novo workflow" submitLabel="Criar workflow" maxWidth="sm" action={createWorkflow}>
          <NewWorkflowFields users={people} />
        </FormDialog>
      </Stack>

      <WorkflowStats stats={totals} />

      <WorkflowFilters filter={filter} search={search} />

      <Card variant="outlined">
        <Box sx={{ overflowX: "auto" }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Título</TableCell>
                <TableCell>Descrição</TableCell>
                <TableCell>Prioridade</TableCell>
                <TableCell>Categoria</TableCell>
                <TableCell>Prazo</TableCell>
                <TableCell>Criação</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {workflows.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8} sx={{ color: inkSoft, textAlign: "center", py: 4 }}>
                    Nenhum workflow encontrado. Crie um novo ou ajuste os filtros.
                  </TableCell>
                </TableRow>
              )}
              {workflows.items.map((workflow) => (
                <TableRow key={workflow.id}>
                  <TableCell sx={{ maxWidth: 260 }}>
                    <TextLink href={`/workflows/${workflow.id}`}>{workflow.title}</TextLink>
                  </TableCell>
                  <TableCell sx={{ color: inkSoft, maxWidth: 260 }}>{excerpt(workflow.description, 50) || "—"}</TableCell>
                  <TableCell>
                    <PriorityChip priority={workflow.priority} />
                  </TableCell>
                  <TableCell>
                    <CategoryChip category={workflow.category} />
                  </TableCell>
                  <TableCell>
                    <DueDate dueDate={workflow.dueDate} closed={workflow.status !== "inProgress"} />
                  </TableCell>
                  <TableCell sx={{ fontVariantNumeric: "tabular-nums", whiteSpace: "nowrap" }}>{formatDateTime(workflow.createdAt)}</TableCell>
                  <TableCell>
                    <StatusChip status={workflow.status} />
                  </TableCell>
                  <TableCell align="right">
                    <Button size="small" href={`/workflows/${workflow.id}`}>
                      Detalhes
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Box>
        <Pager
          basePath="/workflows"
          page={workflows.page}
          pageSize={workflows.pageSize}
          totalCount={workflows.totalCount}
          params={{ search, filter: filter === "all" ? undefined : filter }}
        />
      </Card>
    </Stack>
  );
}
