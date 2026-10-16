import type { DecorationKind, PostKind, RunStatus } from "./types";

export const POST_KINDS: PostKind[] = ["standard", "machine", "quality"];
export const POST_KIND_LABELS: Record<PostKind, string> = {
  standard: "Posto comum",
  machine: "Máquina",
  quality: "Qualidade",
};
export const POST_KIND_HINTS: Record<PostKind, string> = {
  standard: "Uma pessoa por posto e por vez.",
  machine: "A mesma pessoa pode cuidar de até 5 máquinas.",
  quality: "Inspeção ou conferência no fluxo.",
};

export const DECORATION_KINDS: DecorationKind[] = ["belt", "zone", "arrow", "machine", "caution", "rack", "table", "pillar"];
export const DECORATION_LABELS: Record<DecorationKind, string> = {
  belt: "Esteira",
  zone: "Área",
  arrow: "Seta de fluxo",
  machine: "Equipamento",
  caution: "Atenção",
  rack: "Estante",
  table: "Bancada",
  pillar: "Pilar",
};
export const DECORATION_DEFAULTS: Record<DecorationKind, { width: number; height: number; label: string | null }> = {
  belt: { width: 420, height: 60, label: null },
  zone: { width: 360, height: 240, label: "Área" },
  arrow: { width: 120, height: 60, label: null },
  machine: { width: 180, height: 120, label: "Equipamento" },
  caution: { width: 180, height: 60, label: "Atenção" },
  rack: { width: 180, height: 120, label: null },
  table: { width: 240, height: 90, label: null },
  pillar: { width: 60, height: 60, label: null },
};

export const RUN_STATUS_LABELS: Record<RunStatus, string> = {
  preparing: "Em preparo",
  running: "Em produção",
  closed: "Encerrada",
};

export const MAX_MACHINES_PER_PERSON = 5;

export const FIELD_LIMITS = {
  lineName: 60,
  shiftName: 40,
  productCode: 30,
  productName: 120,
  postName: 60,
  operation: 200,
  label: 60,
} as const;
