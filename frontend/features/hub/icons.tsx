import type { SvgIconProps } from "@mui/material/SvgIcon";
import AdminPanelSettings from "@mui/icons-material/AdminPanelSettings";
import AltRoute from "@mui/icons-material/AltRoute";
import Apps from "@mui/icons-material/Apps";
import BarChart from "@mui/icons-material/BarChart";
import Build from "@mui/icons-material/Build";
import CalendarMonth from "@mui/icons-material/CalendarMonth";
import Construction from "@mui/icons-material/Construction";
import Description from "@mui/icons-material/Description";
import EditNote from "@mui/icons-material/EditNote";
import Factory from "@mui/icons-material/Factory";
import FactCheck from "@mui/icons-material/FactCheck";
import Inventory2 from "@mui/icons-material/Inventory2";
import LocalShipping from "@mui/icons-material/LocalShipping";
import ManageAccounts from "@mui/icons-material/ManageAccounts";
import Moving from "@mui/icons-material/Moving";
import PrecisionManufacturing from "@mui/icons-material/PrecisionManufacturing";
import QrCode2 from "@mui/icons-material/QrCode2";
import ReportProblem from "@mui/icons-material/ReportProblem";
import Speed from "@mui/icons-material/Speed";
import Straighten from "@mui/icons-material/Straighten";
import Tune from "@mui/icons-material/Tune";
import Warehouse from "@mui/icons-material/Warehouse";

const ICONS = {
  administracao: ManageAccounts,
  planejamento: CalendarMonth,
  producao: Factory,
  qualidade: FactCheck,
  manutencao: Build,
  logistica: Warehouse,
  rastreabilidade: QrCode2,
  indicadores: BarChart,
  ACE: AdminPanelSettings,
  CFG: Tune,
  PCP: CalendarMonth,
  MRP: Inventory2,
  ROT: AltRoute,
  APT: EditNote,
  LIN: PrecisionManufacturing,
  OEE: Speed,
  INS: FactCheck,
  NCO: ReportProblem,
  MET: Straighten,
  MNT: Build,
  ATV: Construction,
  ALM: Warehouse,
  ABS: Moving,
  EXP: LocalShipping,
  TRC: QrCode2,
  DOC: Description,
  IND: BarChart,
} as const;

export function HubIcon({ name, ...props }: { name: string } & SvgIconProps) {
  const Icon = ICONS[name as keyof typeof ICONS] ?? Apps;
  return <Icon fontSize="small" {...props} />;
}
