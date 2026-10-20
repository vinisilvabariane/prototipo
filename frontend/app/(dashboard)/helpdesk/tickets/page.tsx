import type { Metadata } from "next";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Chip from "@mui/material/Chip";
import DataTable from "@/components/ui/DataTable";
import FilterBar from "@/components/ui/FilterBar";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import TicketStatusChip from "@/features/helpdesk/components/TicketStatusChip";
import { formatElapsed, STATUS_LABELS, STATUS_ORDER } from "@/features/helpdesk/labels";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";
import { formatDateTime } from "@/lib/format";
import { all } from "@/lib/result";

export const metadata: Metadata = {
  title: "Help desk · Chamados",
  description: "Histórico dos chamados que você abriu e dos setores que você atende.",
};

export default async function TicketsPage({ searchParams }: PageProps<"/helpdesk/tickets">) {
  const params = await searchParams;
  const pick = (key: string) => (typeof params[key] === "string" ? (params[key] as string) : "");
  const page = Math.max(0, Number(params.page) || 0);
  const scope = pick("scope") === "attend" ? "attend" : "mine";
  const query = { scope, status: pick("status"), sectorId: pick("sectorId"), from: pick("from"), to: pick("to") } as const;

  const [tickets, sectors] = await Promise.all([helpdeskService.tickets({ ...query, page, pageSize: 15 }), helpdeskService.sectors()]);
  const loaded = all({ tickets, sectors });
  if (!loaded.ok) return <PageError title="Chamados" error={loaded.error} retryHref="/helpdesk/tickets" />;

  const { tickets: data, sectors: sectorList } = loaded.value;
  const canAttendAny = sectorList.items.some((sector) => sector.canAttend);

  return (
    <Stack spacing={2.5}>
      <PageHeader title="Chamados" description={scope === "mine" ? "Os chamados que você abriu." : "Os chamados dos setores que o seu perfil atende."} />

      <FilterBar
        basePath="/helpdesk/tickets"
        values={{ scope, status: query.status, sectorId: query.sectorId, from: query.from, to: query.to }}
        fields={[
          ...(canAttendAny
            ? [
                {
                  type: "segment" as const,
                  name: "scope",
                  label: "Quais chamados",
                  options: [
                    { value: "mine", label: "Que abri" },
                    { value: "attend", label: "Do meu atendimento" },
                  ],
                },
              ]
            : []),
          { type: "select", name: "status", label: "Situação", options: STATUS_ORDER.map((status) => ({ value: status, label: STATUS_LABELS[status] })) },
          { type: "select", name: "sectorId", label: "Setor", options: sectorList.items.map((sector) => ({ value: String(sector.id), label: sector.name })) },
          { type: "date", name: "from", label: "De" },
          { type: "date", name: "to", label: "Até" },
        ]}
      />

      <Card sx={{ overflow: "hidden" }}>
        <DataTable
          rows={data.items}
          rowKey={(ticket) => ticket.id}
          emptyTitle="Nenhum chamado encontrado"
          emptyHint="Mude os filtros ou abra um chamado na primeira aba."
          columns={[
            { key: "id", header: "Nº", fit: true, numeric: true, align: "left", render: (ticket) => <TextLink href={`/helpdesk/tickets/${ticket.id}`}>#{ticket.id}</TextLink> },
            { key: "sector", header: "Setor", strong: true, render: (ticket) => ticket.sectorName },
            {
              key: "summary",
              header: "Resumo",
              hideBelow: "md",
              render: (ticket) => (ticket.summary.length === 0 ? "—" : ticket.summary.map((item) => item.display).join(" · ")),
            },
            {
              key: "place",
              header: "Local",
              hideBelow: "lg",
              render: (ticket) => (
                <>
                  {ticket.lineName ? (ticket.postName ? `${ticket.lineName} · ${ticket.postName}` : ticket.lineName) : "—"}
                  {ticket.isLineStopped && ticket.status !== "rejected" && <Chip size="small" color="error" label="Parada" sx={{ ml: 1 }} />}
                </>
              ),
            },
            { key: "status", header: "Situação", fit: true, render: (ticket) => <TicketStatusChip status={ticket.status} /> },
            { key: "openedAt", header: "Aberto em", fit: true, numeric: true, align: "left", hideBelow: "md", render: (ticket) => formatDateTime(ticket.openedAt) },
            {
              key: "elapsed",
              header: "Tempo",
              fit: true,
              numeric: true,
              hideBelow: "lg",
              render: (ticket) => formatElapsed(ticket.openedAt, ticket.finishedAt),
            },
          ]}
        />
        <Pager basePath="/helpdesk/tickets" page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} params={{ scope, status: query.status, sectorId: query.sectorId, from: query.from, to: query.to }} />
      </Card>
    </Stack>
  );
}
