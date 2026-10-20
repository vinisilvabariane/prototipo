import type { Metadata } from "next";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import EmptyState from "@/components/ui/EmptyState";
import FilterBar from "@/components/ui/FilterBar";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import TicketCard from "@/features/helpdesk/components/TicketCard";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";
import type { TicketSummary } from "@/features/helpdesk/types";
import AutoRefresh from "@/features/production/runs/AutoRefresh";
import { all } from "@/lib/result";
import { inkSoft, line } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Help desk · Quadro",
  description: "Chamados abertos e em atendimento dos setores que você atende.",
};

function byUrgency(a: TicketSummary, b: TicketSummary) {
  if (a.isLineStopped !== b.isLineStopped) return a.isLineStopped ? -1 : 1;
  return new Date(a.openedAt).getTime() - new Date(b.openedAt).getTime();
}

function Column({ title, tickets, empty }: { title: string; tickets: TicketSummary[]; empty: string }) {
  return (
    <Box sx={{ minWidth: 0 }}>
      <Stack direction="row" spacing={1} sx={{ alignItems: "baseline", pb: 1, mb: 1.5, borderBottom: `1px solid ${line}` }}>
        <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
          {title}
        </Typography>
        <Typography variant="overline" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
          {tickets.length}
        </Typography>
      </Stack>
      <Stack spacing={1.5}>
        {tickets.length === 0 ? (
          <Typography variant="body2" sx={{ color: inkSoft, py: 2 }}>
            {empty}
          </Typography>
        ) : (
          tickets.map((ticket) => <TicketCard key={ticket.id} ticket={ticket} />)
        )}
      </Stack>
    </Box>
  );
}

export default async function BoardPage({ searchParams }: PageProps<"/helpdesk/board">) {
  const params = await searchParams;
  const sectorId = typeof params.sectorId === "string" ? params.sectorId : "";

  const [open, inProgress, sectors] = await Promise.all([
    helpdeskService.tickets({ status: "open", sectorId, pageSize: 50 }),
    helpdeskService.tickets({ status: "inProgress", sectorId, pageSize: 50 }),
    helpdeskService.sectors(),
  ]);

  const loaded = all({ open, inProgress, sectors });
  if (!loaded.ok) return <PageError title="Quadro de atendimento" error={loaded.error} retryHref="/helpdesk/board" />;

  const { open: openPage, inProgress: progressPage, sectors: sectorList } = loaded.value;

  if (!openPage.canAttend) {
    return (
      <Stack spacing={2.5}>
        <PageHeader title="Quadro de atendimento" />
        <Card>
          <EmptyState title="Seu perfil não atende nenhum setor" hint="O quadro mostra os chamados dos setores em que o seu perfil faz atendimento. Para acompanhar o que você abriu, use a aba Chamados." />
        </Card>
      </Stack>
    );
  }

  const stopped = [...openPage.items, ...progressPage.items].filter((ticket) => ticket.isLineStopped).length;

  return (
    <Stack spacing={2.5}>
      <AutoRefresh />
      <PageHeader
        title="Quadro de atendimento"
        description={`${openPage.counts.open} aberto(s) e ${progressPage.counts.inProgress} em atendimento${stopped > 0 ? ` · ${stopped} com linha parada` : ""}. Atualiza sozinho a cada 30 segundos.`}
      />

      <FilterBar
        basePath="/helpdesk/board"
        values={{ sectorId }}
        fields={[{ type: "select", name: "sectorId", label: "Setor", options: sectorList.items.filter((sector) => sector.canAttend).map((sector) => ({ value: String(sector.id), label: sector.name })) }]}
      />

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" }, alignItems: "start" }}>
        <Column title="Abertos" tickets={[...openPage.items].sort(byUrgency)} empty="Nenhum chamado esperando atendimento." />
        <Column title="Em atendimento" tickets={[...progressPage.items].sort(byUrgency)} empty="Ninguém está atendendo agora." />
      </Box>
    </Stack>
  );
}
