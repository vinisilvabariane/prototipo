import Chip from "@mui/material/Chip";
import { CRITICALITY_LABELS, QUALIFICATION_STATUS_LABELS } from "../labels";
import type { QualificationStatus, SupplierCriticality, SupplierQualification } from "../types";

const STATUS_STYLE = {
  approved: { variant: "filled", color: "primary" },
  rejected: { variant: "filled", color: "error" },
  discontinued: { variant: "filled", color: "default" },
} as const;

const CRITICALITY_COLOR = { high: "error", medium: "warning", low: "default" } as const;

export function StatusChip({ status, expired }: { status: QualificationStatus; expired?: boolean }) {
  if (expired) return <Chip size="small" variant="outlined" color="warning" label="Vencido" />;

  const style = STATUS_STYLE[status];
  return <Chip size="small" variant={style.variant} color={style.color} label={QUALIFICATION_STATUS_LABELS[status]} />;
}

export function QualificationChip({ qualification }: { qualification: SupplierQualification | null }) {
  if (!qualification) return <Chip size="small" variant="outlined" label="Não avaliado" />;
  return <StatusChip status={qualification.status} expired={qualification.expired} />;
}

export function CriticalityChip({ criticality }: { criticality: SupplierCriticality | null }) {
  if (!criticality) return <>—</>;
  return <Chip size="small" variant="outlined" color={CRITICALITY_COLOR[criticality]} label={CRITICALITY_LABELS[criticality]} />;
}
