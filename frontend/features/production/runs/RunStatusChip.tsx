import Chip from "@mui/material/Chip";
import { RUN_STATUS_LABELS } from "../labels";
import type { RunStatus } from "../types";

const STYLE = {
  preparing: { variant: "outlined", color: "default" },
  running: { variant: "filled", color: "primary" },
  closed: { variant: "filled", color: "default" },
} as const;

export default function RunStatusChip({ status }: { status: RunStatus }) {
  const style = STYLE[status];
  return <Chip size="small" variant={style.variant} color={style.color} label={RUN_STATUS_LABELS[status]} />;
}
