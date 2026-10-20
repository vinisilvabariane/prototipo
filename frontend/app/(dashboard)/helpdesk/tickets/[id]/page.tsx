import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Panel from "@/components/ui/Panel";
import ResolvePanel from "@/features/helpdesk/components/ResolvePanel";
import TicketActions from "@/features/helpdesk/components/TicketActions";
import TicketStatusChip from "@/features/helpdesk/components/TicketStatusChip";
import { formatDuration } from "@/features/helpdesk/labels";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";
import type { AnswerView } from "@/features/helpdesk/types";
import AutoRefresh from "@/features/production/runs/AutoRefresh";
import { formatDateTime } from "@/lib/format";
import { inkSoft, line } from "@/lib/tokens";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/helpdesk/tickets/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Chamado não encontrado" };

  const result = await helpdeskService.ticket(id);
  if (result.ok) return { title: `Chamado #${result.value.id} · ${result.value.sectorName}` };
  return { title: result.error.code === "notFound" ? "Chamado não encontrado" : "Chamado" };
}

function Answers({ items, empty }: { items: AnswerView[]; empty: string }) {
  if (items.length === 0) {
    return (
      <Typography variant="body2" sx={{ color: inkSoft }}>
        {empty}
      </Typography>
    );
  }

  return (
    <Box component="dl" sx={{ m: 0, display: "grid", gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "minmax(0, 220px) minmax(0, 1fr)" }, columnGap: 3, rowGap: 1.25 }}>
      {items.map((item) => (
        <Box key={item.label} sx={{ display: "contents" }}>
          <Typography component="dt" variant="body2" sx={{ color: inkSoft }}>
            {item.label}
          </Typography>
          <Typography component="dd" variant="body2" sx={{ m: 0, fontWeight: 600, overflowWrap: "anywhere", whiteSpace: "pre-wrap" }}>
            {item.display}
            {item.at && <Box component="span" sx={{ color: inkSoft, fontWeight: 400 }}> · desde {formatDateTime(item.at)}</Box>}
          </Typography>
        </Box>
      ))}
    </Box>
  );
}

function Step({ label, at, by, done }: { label: string; at: string | null; by?: string | null; done: boolean }) {
  return (
    <Stack direction="row" spacing={1.5} sx={{ alignItems: "flex-start", opacity: done ? 1 : 0.45 }}>
      <Box aria-hidden sx={{ width: 10, height: 10, borderRadius: "50%", mt: 0.75, flexShrink: 0, bgcolor: done ? "primary.main" : "transparent", border: `2px solid ${done ? "var(--mui-palette-primary-main)" : line}` }} />
      <Box>
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {label}
          {by && <Box component="span" sx={{ fontWeight: 400, color: inkSoft }}> · {by}</Box>}
        </Typography>
        <Typography variant="caption" sx={{ color: inkSoft }}>
          {at ? formatDateTime(at) : "—"}
        </Typography>
      </Box>
    </Stack>
  );
}

export default async function TicketPage({ params }: PageProps<"/helpdesk/tickets/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const result = await helpdeskService.ticket(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Chamado" error={result.error} retryHref={`/helpdesk/tickets/${id}`} />;

  const ticket = result.value;
  const active = ticket.status === "open" || ticket.status === "inProgress";

  return (
    <Stack spacing={2.5}>
      {active && <AutoRefresh />}

      <PageHeader
        back={{ href: "/helpdesk/tickets", label: "Chamados" }}
        eyebrow={`Chamado #${ticket.id}`}
        title={ticket.sectorName}
        badges={
          <>
            <TicketStatusChip status={ticket.status} />
            {ticket.isLineStopped && ticket.status !== "rejected" && <Chip size="small" color="error" label="Linha parada" />}
          </>
        }
        description={`Aberto por ${ticket.openedByName} em ${formatDateTime(ticket.openedAt)}${ticket.lineName ? ` · ${ticket.postName ? `${ticket.lineName} · ${ticket.postName}` : ticket.lineName}` : ""}`}
        actions={<TicketActions id={ticket.id} canStart={ticket.canStart} canReject={ticket.canReject} />}
      />

      {ticket.status === "rejected" && (
        <Alert severity="warning" variant="outlined">
          Recusado por {ticket.rejectedByName ?? "—"}: {ticket.rejectReason}
        </Alert>
      )}

      <Box sx={{ display: "grid", gap: 2.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "minmax(0, 1fr) 320px" }, alignItems: "start" }}>
        <Stack spacing={2.5}>
          <Panel title="O que foi informado">
            <Answers items={ticket.answers} empty="Nenhuma resposta registrada." />
          </Panel>

          {ticket.canResolve && (
            <Panel title="Finalizar chamado">
              <ResolvePanel id={ticket.id} form={ticket.resolutionForm} />
            </Panel>
          )}

          {ticket.status === "resolved" && (
            <Panel title="Resposta do atendimento">
              <Stack spacing={2}>
                <Answers items={ticket.resolutionAnswers} empty="O setor não pediu formulário de resposta." />
                {ticket.note && (
                  <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
                    <Box component="span" sx={{ color: inkSoft }}>Observação: </Box>
                    {ticket.note}
                  </Typography>
                )}
              </Stack>
            </Panel>
          )}
        </Stack>

        <Stack spacing={2.5}>
          <Panel title="Andamento">
            <Stack spacing={2}>
              <Step label="Aberto" at={ticket.openedAt} by={ticket.openedByName} done />
              <Step label="Assumido" at={ticket.startedAt} by={ticket.assignedToName} done={ticket.startedAt !== null} />
              {ticket.status === "rejected" ? (
                <Step label="Recusado" at={ticket.finishedAt} by={ticket.rejectedByName} done />
              ) : (
                <Step label="Resolvido" at={ticket.finishedAt} done={ticket.status === "resolved"} />
              )}
            </Stack>
          </Panel>

          {(ticket.isLineStopped || ticket.defects > 0) && ticket.status !== "rejected" && (
            <Panel title="Impacto na produção">
              <Stack spacing={1}>
                {ticket.isLineStopped && (
                  <Typography variant="body2">
                    Linha parada {ticket.status === "resolved" ? "por" : "há"} <strong>{formatDuration(ticket.downtimeSeconds)}</strong>
                    {ticket.status !== "resolved" && " (ainda contando)"}
                  </Typography>
                )}
                {ticket.defects > 0 && (
                  <Typography variant="body2">
                    <strong>{ticket.defects}</strong> peça(s) com defeito
                  </Typography>
                )}
                <Typography variant="caption" sx={{ color: inkSoft }}>
                  Esses números entram no OEE da linha.
                </Typography>
              </Stack>
            </Panel>
          )}
        </Stack>
      </Box>
    </Stack>
  );
}
