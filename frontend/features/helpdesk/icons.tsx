import type { SvgIconProps } from "@mui/material/SvgIcon";
import BugReport from "@mui/icons-material/BugReport";
import Bolt from "@mui/icons-material/Bolt";
import Build from "@mui/icons-material/Build";
import Code from "@mui/icons-material/Code";
import Engineering from "@mui/icons-material/Engineering";
import FactCheck from "@mui/icons-material/FactCheck";
import Handyman from "@mui/icons-material/Handyman";
import Inventory2 from "@mui/icons-material/Inventory2";
import LocalShipping from "@mui/icons-material/LocalShipping";
import Memory from "@mui/icons-material/Memory";
import PrecisionManufacturing from "@mui/icons-material/PrecisionManufacturing";
import ReportProblem from "@mui/icons-material/ReportProblem";
import Science from "@mui/icons-material/Science";
import Settings from "@mui/icons-material/Settings";
import SupportAgent from "@mui/icons-material/SupportAgent";
import WarningAmber from "@mui/icons-material/WarningAmber";

const ICONS = {
  support: SupportAgent,
  build: Build,
  bolt: Bolt,
  memory: Memory,
  inventory: Inventory2,
  bug: BugReport,
  code: Code,
  science: Science,
  engineering: Engineering,
  settings: Settings,
  report: ReportProblem,
  warning: WarningAmber,
  shipping: LocalShipping,
  handyman: Handyman,
  precision: PrecisionManufacturing,
  quality: FactCheck,
} as const;

export const ICON_KEYS = Object.keys(ICONS) as (keyof typeof ICONS)[];

export const ICON_LABELS: Record<(typeof ICON_KEYS)[number], string> = {
  support: "Atendimento",
  build: "Manutenção",
  bolt: "Elétrica",
  memory: "Eletrônica",
  inventory: "Estoque",
  bug: "Software",
  code: "Código",
  science: "Laboratório",
  engineering: "Engenharia",
  settings: "Ajuste",
  report: "Ocorrência",
  warning: "Atenção",
  shipping: "Logística",
  handyman: "Reparo",
  precision: "Automação",
  quality: "Qualidade",
};

export function HelpdeskIcon({ name, ...props }: { name: string | null | undefined } & SvgIconProps) {
  const Icon = ICONS[(name ?? "support") as keyof typeof ICONS] ?? SupportAgent;
  return <Icon {...props} />;
}
