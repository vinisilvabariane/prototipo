import type { jsPDF } from "jspdf";
import { formatBytes, formatDateTime, formatDueDate } from "./format";
import { ACTION_TITLES, CATEGORY_LABELS, PRIORITY_LABELS, STATUS_LABELS } from "./labels";
import type { WorkflowDetail, WorkflowMessage } from "./types";

const PAGE_WIDTH = 210;
const PAGE_HEIGHT = 297;
const MARGIN = 15;
const FOOTER = 14;
const CONTENT_WIDTH = PAGE_WIDTH - MARGIN * 2;

type Rgb = [number, number, number];

const INK: Rgb = [30, 30, 30];
const SOFT: Rgb = [110, 110, 110];
const BAR: Rgb = [20, 20, 20];
const PANEL: Rgb = [235, 235, 235];

const clean = (text: string) =>
  text
    .replace(/\s*→\s*/g, " para ")
    .replace(/[^\t\n -~ -ÿ–—‘’“”•…]/g, "?");

class Page {
  y = MARGIN;

  constructor(private readonly doc: jsPDF) {}

  ensure(height: number) {
    if (this.y + height <= PAGE_HEIGHT - FOOTER) return;
    this.doc.addPage();
    this.y = MARGIN;
  }

  text(value: string, { size = 10, bold = false, color = INK, indent = 0, after = 1.5 }: { size?: number; bold?: boolean; color?: Rgb; indent?: number; after?: number } = {}) {
    this.doc.setFont("helvetica", bold ? "bold" : "normal");
    this.doc.setFontSize(size);
    this.doc.setTextColor(...color);

    const lineHeight = size * 0.45;
    const lines: string[] = this.doc.splitTextToSize(clean(value), CONTENT_WIDTH - indent);

    for (const line of lines) {
      this.ensure(lineHeight);
      this.doc.text(line, MARGIN + indent, this.y + lineHeight * 0.8);
      this.y += lineHeight;
    }

    this.y += after;
  }

  bar(title: string, fill: Rgb = BAR, color: Rgb = [255, 255, 255]) {
    this.ensure(14);
    this.doc.setFillColor(...fill);
    this.doc.rect(MARGIN, this.y, CONTENT_WIDTH, 8, "F");
    this.doc.setFont("helvetica", "bold");
    this.doc.setFontSize(10);
    this.doc.setTextColor(...color);
    this.doc.text(clean(title), MARGIN + 3, this.y + 5.4);
    this.y += 11;
  }

  field(label: string, value: string) {
    this.ensure(7);
    this.doc.setFont("helvetica", "bold");
    this.doc.setFontSize(10);
    this.doc.setTextColor(...SOFT);
    this.doc.text(clean(label), MARGIN, this.y + 4);

    const lines: string[] = this.doc.splitTextToSize(clean(value || "—"), CONTENT_WIDTH - 40);
    this.doc.setFont("helvetica", "normal");
    this.doc.setTextColor(...INK);

    for (const line of lines) {
      this.ensure(5);
      this.doc.text(line, MARGIN + 40, this.y + 4);
      this.y += 5;
    }

    this.y += 1.5;
  }

  gap(height: number) {
    this.y += height;
  }
}

function writeStep(page: Page, workflow: WorkflowDetail, index: number, messages: WorkflowMessage[]) {
  const step = workflow.steps[index];

  page.bar(`ATIVIDADE ${workflow.steps.length - index}: ${ACTION_TITLES[step.action]}`, PANEL, INK);
  page.text(`Data/Hora: ${formatDateTime(step.createdAt)}    Responsável: ${step.userName}`, { size: 9, color: SOFT });
  page.text(step.description, { size: 10 });

  if (step.justification) {
    page.text("Justificativa:", { size: 9, bold: true, color: SOFT, after: 0.5 });
    page.text(step.justification, { indent: 4 });
  }

  if (messages.length > 0) {
    page.text("Histórico de comunicação:", { size: 9, bold: true, color: SOFT, after: 0.5 });
    for (const message of messages) {
      page.text(`${message.userName} - ${formatDateTime(message.createdAt)}`, { size: 8, bold: true, color: SOFT, indent: 4, after: 0 });
      page.text(message.text, { indent: 8 });
    }
  }

  page.gap(4);
}

export async function saveWorkflowPdf(workflow: WorkflowDetail, messagesByStep: Map<number, WorkflowMessage[]>) {
  const { jsPDF: Pdf } = await import("jspdf");
  const doc = new Pdf({ orientation: "portrait", unit: "mm", format: "a4" });
  const page = new Page(doc);

  page.text("RELATÓRIO TÉCNICO - WORKFLOW", { size: 16, bold: true, after: 1 });
  page.text(`Gerado em: ${formatDateTime(new Date().toISOString())}`, { size: 9, color: SOFT, after: 6 });

  page.bar("INFORMAÇÕES GERAIS");
  page.field("Número:", String(workflow.id));
  page.field("Título:", workflow.title);
  page.field("Descrição:", workflow.description || "Sem descrição");
  page.field("Status:", STATUS_LABELS[workflow.status]);
  page.field("Prioridade:", PRIORITY_LABELS[workflow.priority]);
  page.field("Categoria:", workflow.category ? CATEGORY_LABELS[workflow.category] : "Sem categoria");
  page.field("Prazo final:", formatDueDate(workflow.dueDate));
  page.field("Criado por:", workflow.creatorName);
  page.field("Data de criação:", formatDateTime(workflow.createdAt));
  if (workflow.completedAt) page.field("Concluído em:", formatDateTime(workflow.completedAt));
  page.gap(4);

  page.bar("RESPONSÁVEIS");
  workflow.assignees.forEach((assignee, index) => {
    page.text(`${index + 1}. ${assignee.name} (matrícula ${assignee.registration}) - ${assignee.approved ? "aprovou" : "pendente"}`, { after: 1 });
  });
  page.gap(4);

  page.bar("ANEXOS");
  if (workflow.attachments.length === 0) page.text("Nenhum documento anexado.", { color: SOFT });
  workflow.attachments.forEach((attachment, index) => {
    page.text(`${index + 1}. ${attachment.name}  (${formatBytes(attachment.size)}, ${formatDateTime(attachment.uploadedAt)})`, { after: 1 });
  });
  page.gap(4);

  page.bar("HISTÓRICO DE ATIVIDADES");
  if (workflow.steps.length === 0) page.text("Nenhuma atividade registrada.", { color: SOFT });
  workflow.steps.forEach((step, index) => writeStep(page, workflow, index, messagesByStep.get(step.id) ?? []));

  const total = doc.getNumberOfPages();
  for (let current = 1; current <= total; current++) {
    doc.setPage(current);
    doc.setFont("helvetica", "normal");
    doc.setFontSize(8);
    doc.setTextColor(...SOFT);
    doc.text(`Workflow #${workflow.id}`, MARGIN, PAGE_HEIGHT - 8);
    doc.text(`Página ${current} de ${total}`, PAGE_WIDTH / 2, PAGE_HEIGHT - 8, { align: "center" });
    doc.text("MeOne", PAGE_WIDTH - MARGIN, PAGE_HEIGHT - 8, { align: "right" });
  }

  const slug = workflow.title.slice(0, 30).replace(/[^a-zA-Z0-9]/g, "_");
  const day = new Date().toISOString().slice(0, 10);
  doc.save(`Relatorio_Workflow_${workflow.id}_${slug}_${day}.pdf`);
}
