import Chip from "@mui/material/Chip";
import { STATUS_LABELS } from "../labels";
import type { TicketStatus } from "../types";

const STYLE = {
  open: { variant: "outlined", color: "default" },
  inProgress: { variant: "filled", color: "primary" },
  resolved: { variant: "filled", color: "success" },
  rejected: { variant: "filled", color: "default" },
} as const;

export default function TicketStatusChip({ status }: { status: TicketStatus }) {
  const style = STYLE[status];
  return <Chip size="small" variant={style.variant} color={style.color} label={STATUS_LABELS[status]} />;
}
