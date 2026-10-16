import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import { CATEGORY_LABELS, PRIORITY_LABELS, STATUS_LABELS } from "../labels";
import { dueTone, formatDueDate } from "../format";
import type { WorkflowCategory, WorkflowPriority, WorkflowStatus } from "../types";

const STATUS_STYLE = {
  inProgress: { variant: "outlined", color: "default" },
  approved: { variant: "filled", color: "primary" },
  rejected: { variant: "filled", color: "error" },
} as const;

const PRIORITY_COLOR = { high: "error", medium: "warning", low: "default" } as const;

export function StatusChip({ status }: { status: WorkflowStatus }) {
  const style = STATUS_STYLE[status];
  return <Chip size="small" label={STATUS_LABELS[status]} variant={style.variant} color={style.color} />;
}

export function PriorityChip({ priority }: { priority: WorkflowPriority }) {
  return <Chip size="small" variant="outlined" label={PRIORITY_LABELS[priority]} color={PRIORITY_COLOR[priority]} />;
}

export function CategoryChip({ category }: { category: WorkflowCategory | null }) {
  if (!category) return <Box component="span">—</Box>;
  return <Chip size="small" variant="outlined" label={CATEGORY_LABELS[category]} />;
}

const TONE_COLOR = { overdue: "error.main", today: "warning.main", upcoming: "text.primary", none: "text.secondary" } as const;

export function DueDate({ dueDate, closed }: { dueDate: string | null; closed: boolean }) {
  const tone = dueTone(dueDate, closed);
  return (
    <Box component="span" sx={{ color: TONE_COLOR[tone], fontWeight: tone === "overdue" || tone === "today" ? 600 : 400, fontVariantNumeric: "tabular-nums" }}>
      {formatDueDate(dueDate)}
    </Box>
  );
}
