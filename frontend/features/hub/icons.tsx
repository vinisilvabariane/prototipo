import type { SvgIconProps } from "@mui/material/SvgIcon";
import AccountTree from "@mui/icons-material/AccountTree";
import Assignment from "@mui/icons-material/Assignment";
import AdminPanelSettings from "@mui/icons-material/AdminPanelSettings";
import AltRoute from "@mui/icons-material/AltRoute";
import Apps from "@mui/icons-material/Apps";
import BarChart from "@mui/icons-material/BarChart";
import Build from "@mui/icons-material/Build";
import CalendarMonth from "@mui/icons-material/CalendarMonth";
import Construction from "@mui/icons-material/Construction";
import Description from "@mui/icons-material/Description";
import EditNote from "@mui/icons-material/EditNote";
import Engineering from "@mui/icons-material/Engineering";
import Factory from "@mui/icons-material/Factory";
import FactCheck from "@mui/icons-material/FactCheck";
import Handshake from "@mui/icons-material/Handshake";
import Inventory2 from "@mui/icons-material/Inventory2";
import SupportAgent from "@mui/icons-material/SupportAgent";
import LocalShipping from "@mui/icons-material/LocalShipping";
import ManageAccounts from "@mui/icons-material/ManageAccounts";
import Moving from "@mui/icons-material/Moving";
import PrecisionManufacturing from "@mui/icons-material/PrecisionManufacturing";
import QrCode2 from "@mui/icons-material/QrCode2";
import Schema from "@mui/icons-material/Schema";
import ReportProblem from "@mui/icons-material/ReportProblem";
import Speed from "@mui/icons-material/Speed";
import Straighten from "@mui/icons-material/Straighten";
import Tune from "@mui/icons-material/Tune";
import Warehouse from "@mui/icons-material/Warehouse";

const ICONS = {
  administracao: ManageAccounts,
  planejamento: CalendarMonth,
  engenharia: Engineering,
  producao: Factory,
  qualidade: FactCheck,
  manutencao: Build,
  logistica: Warehouse,
  rastreabilidade: QrCode2,
  indicadores: BarChart,
  ACE: AdminPanelSettings,
  CFG: Tune,
  ORG: Schema,
  WFL: AccountTree,
  INT: Assignment,
  PCP: CalendarMonth,
  MRP: Inventory2,
  ROT: AltRoute,
  APT: EditNote,
  LIN: PrecisionManufacturing,
  OEE: Speed,
  HLP: SupportAgent,
  FOR: Handshake,
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
