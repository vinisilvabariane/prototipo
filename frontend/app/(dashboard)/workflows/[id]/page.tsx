import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Panel from "@/components/ui/Panel";
import { approveWorkflow, rejectWorkflow, updateWorkflow } from "@/features/workflows/actions/workflowActions";
import AssigneesCard from "@/features/workflows/components/AssigneesCard";
import AttachmentsCard from "@/features/workflows/components/AttachmentsCard";
import JustificationField from "@/features/workflows/components/JustificationField";
import StepTimeline from "@/features/workflows/components/StepTimeline";
import { CategoryChip, DueDate, PriorityChip, StatusChip } from "@/features/workflows/components/WorkflowBadges";
import WorkflowFields from "@/features/workflows/components/WorkflowFields";
import WorkflowPdfButton from "@/features/workflows/components/WorkflowPdfButton";
import { formatDateTime } from "@/features/workflows/format";
import { workflowService } from "@/features/workflows/services/WorkflowService";
import { ok } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import Field from "@/components/ui/Field";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/workflows/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Workflow não encontrado" };

  const result = await workflowService.getById(id);
  if (result.ok) return { title: result.value.title };
  return { title: result.error.code === "notFound" ? "Workflow não encontrado" : "Workflow" };
}

export default async function WorkflowPage({ params }: PageProps<"/workflows/[id]">) {
  const workflowId = parseId((await params).id);
  if (workflowId === null) notFound();

  const loaded = await workflowService.getById(workflowId);
  if (!loaded.ok && loaded.error.code === "notFound") notFound();
  if (!loaded.ok) return <PageError title="Workflow" error={loaded.error} retryHref={`/workflows/${workflowId}`} />;

  const workflow = loaded.value;
  const { permissions } = workflow;
  const closed = workflow.status !== "inProgress";

  const users = permissions.canManageAssignees ? await workflowService.users() : ok([]);
  if (!users.ok) return <PageError title="Workflow" error={users.error} retryHref={`/workflows/${workflowId}`} />;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        back={{ href: "/workflows", label: "Workflows" }}
        eyebrow={`Workflow #${workflow.id}`}
        title={workflow.title}
        badges={
          <>
            <StatusChip status={workflow.status} />
            <PriorityChip priority={workflow.priority} />
            <CategoryChip category={workflow.category} />
          </>
        }
        description={
          <>
            Prazo final: <DueDate dueDate={workflow.dueDate} closed={closed} />
          </>
        }
        actions={
          <>
            {permissions.canApprove && (
              <FormDialog title="Aprovar workflow" trigger="Aprovar" submitLabel="Confirmar aprovação" action={approveWorkflow.bind(null, workflow.id)}>
                <JustificationField />
              </FormDialog>
            )}
            {permissions.canReject && (
              <FormDialog title="Rejeitar workflow" trigger="Rejeitar" color="error" submitLabel="Confirmar rejeição" action={rejectWorkflow.bind(null, workflow.id)}>
                <JustificationField />
              </FormDialog>
            )}
            {permissions.canEdit && (
              <FormDialog title="Editar workflow" trigger="Editar" variant="outlined" maxWidth="sm" action={updateWorkflow.bind(null, workflow.id)}>
                <WorkflowFields workflow={workflow} />
              </FormDialog>
            )}
            <WorkflowPdfButton workflow={workflow} />
          </>
        }
      />

      <Box sx={{ display: "grid", gap: 2.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 2fr) minmax(0, 1fr)" }, alignItems: "start" }}>
        <Panel title="Descrição">
          <Typography sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere", color: workflow.description ? "text.primary" : inkSoft, fontStyle: workflow.description ? "normal" : "italic" }}>
            {workflow.description || "Sem descrição"}
          </Typography>
        </Panel>

        <Panel title="Detalhes">
          <Field label="Criado por">{workflow.creatorName}</Field>
          <Field label="Data de criação">{formatDateTime(workflow.createdAt)}</Field>
          {workflow.completedAt && <Field label="Concluído em">{formatDateTime(workflow.completedAt)}</Field>}
        </Panel>
      </Box>

      <Box sx={{ display: "grid", gap: 2.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 5fr) minmax(0, 7fr)" }, alignItems: "start" }}>
        <AssigneesCard
          workflowId={workflow.id}
          creatorId={workflow.creatorId}
          assignees={workflow.assignees}
          candidates={users.value}
          canManage={permissions.canManageAssignees}
        />

        <Panel title="Histórico de ações">
          <StepTimeline workflowId={workflow.id} steps={workflow.steps} />
        </Panel>
      </Box>

      <AttachmentsCard workflowId={workflow.id} attachments={workflow.attachments} />
    </Stack>
  );
}
