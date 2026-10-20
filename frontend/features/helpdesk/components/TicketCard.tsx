import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import TextLink from "@/components/ui/TextLink";
import { inkSoft } from "@/lib/tokens";
import { HelpdeskIcon } from "../icons";
import { formatElapsed } from "../labels";
import type { TicketSummary } from "../types";
import TicketActions from "./TicketActions";

export default function TicketCard({ ticket }: { ticket: TicketSummary }) {
  const stopped = ticket.isLineStopped && ticket.status !== "rejected";

  return (
    <Card sx={{ p: 2, display: "flex", flexDirection: "column", gap: 1.25, borderLeft: `4px solid ${stopped ? "var(--mui-palette-error-main)" : ticket.sectorColor}` }}>
      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        <Box sx={{ width: 28, height: 28, borderRadius: 1, display: "grid", placeItems: "center", color: "#fff", bgcolor: ticket.sectorColor, flexShrink: 0 }}>
          <HelpdeskIcon name={ticket.sectorIcon} sx={{ fontSize: 18 }} />
        </Box>
        <Typography variant="body2" sx={{ fontWeight: 700, flex: 1, minWidth: 0 }} noWrap>
          <TextLink href={`/helpdesk/tickets/${ticket.id}`}>
            #{ticket.id} · {ticket.sectorName}
          </TextLink>
        </Typography>
        <Typography variant="caption" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
          {formatElapsed(ticket.openedAt)}
        </Typography>
      </Stack>

      {(ticket.lineName || stopped) && (
        <Stack direction="row" spacing={0.75} sx={{ flexWrap: "wrap", rowGap: 0.5, alignItems: "center" }}>
          {ticket.lineName && <Chip size="small" variant="outlined" label={ticket.postName ? `${ticket.lineName} · ${ticket.postName}` : ticket.lineName} />}
          {stopped && <Chip size="small" color="error" label={`Linha parada há ${formatElapsed(ticket.stoppedAt)}`} />}
        </Stack>
      )}

      {ticket.summary.map((item) => (
        <Typography key={item.label} variant="body2" sx={{ overflowWrap: "anywhere" }}>
          <Box component="span" sx={{ color: inkSoft }}>
            {item.label.replace(/[?:]+$/, "")}:{" "}
          </Box>
          {item.display}
        </Typography>
      ))}

      <Typography variant="caption" sx={{ color: inkSoft }}>
        {ticket.openedByName}
        {ticket.assignedToName && ` · atendido por ${ticket.assignedToName}`}
      </Typography>

      <TicketActions id={ticket.id} canStart={ticket.canStart} canReject={ticket.canReject} compact />
    </Card>
  );
}
