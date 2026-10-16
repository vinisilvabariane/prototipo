import type {
  ReviewStage,
  WorkflowAction,
  WorkflowCategory,
  WorkflowFilter,
  WorkflowPriority,
  WorkflowStatus,
} from "./types";

export const STATUS_LABELS: Record<WorkflowStatus, string> = {
  inProgress: "Em andamento",
  approved: "Aprovado",
  rejected: "Rejeitado",
};

export const PRIORITY_LABELS: Record<WorkflowPriority, string> = {
  high: "Alta",
  medium: "Média",
  low: "Baixa",
};

export const CATEGORY_LABELS: Record<WorkflowCategory, string> = {
  requirements: "Requisitos de clientes",
  standards: "Normas SGQ",
  procedures: "Procedimentos",
};

export const FILTER_LABELS: Record<WorkflowFilter, string> = {
  all: "Todos",
  mine: "Meus workflows",
  participating: "Participando",
};

export const ACTION_TITLES: Record<WorkflowAction, string> = {
  created: "Workflow criado",
  edited: "Workflow atualizado",
  approval: "Aprovado por um responsável",
  rejection: "Rejeitado por um responsável",
  finalApproval: "Workflow aprovado",
  finalRejection: "Workflow rejeitado",
  assigneeAdded: "Responsável adicionado",
  assigneeReplaced: "Responsável substituído",
};

export const REVIEW_NOTICES: Record<Exclude<ReviewStage, "none">, string> = {
  creatorReview: "O criador do workflow precisa analisar esta rejeição.",
  assigneeReview: "Quem rejeitou precisa revisar esta rejeição.",
};

export const PRIORITIES = Object.keys(PRIORITY_LABELS) as WorkflowPriority[];

export const CATEGORIES = Object.keys(CATEGORY_LABELS) as WorkflowCategory[];

export const MAX_ATTACHMENTS = 5;

export const MAX_ATTACHMENT_BYTES = 10 * 1024 * 1024;

export const MAX_TITLE_LENGTH = 100;

export const MAX_DESCRIPTION_LENGTH = 500;

export const MAX_JUSTIFICATION_LENGTH = 1000;
