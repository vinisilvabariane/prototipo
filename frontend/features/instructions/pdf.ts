import type { jsPDF } from "jspdf";
import { formatDateTime } from "@/lib/format";

const WIDTH = 210;
const HEIGHT = 297;
const MARGIN = 15;
const FOOTER = 14;
const CONTENT = WIDTH - MARGIN * 2;
const HEADER = 20;
const MAX_IMAGE_SIDE = 1600;
const IMAGE_BOX = { width: 130, height: 85 };

type Rgb = [number, number, number];

const INK: Rgb = [28, 28, 28];
const SOFT: Rgb = [105, 105, 105];
const BAND: Rgb = [20, 20, 20];
const PANEL: Rgb = [238, 238, 238];
const WARNING: Rgb = [255, 244, 214];

export type PdfStep = { text: string; image: Blob | null };

export type PdfInput = {
  code: string;
  title: string;
  documentNumber: string;
  product: string;
  preparedBy: string;
  approvedBy: string;
  requiredPpe: string;
  processSpecifications: string;
  notes: string;
  steps: PdfStep[];
};

type Picture = { data: string; width: number; height: number };

const clean = (value: string) => value.replace(/[^\t\n\u0020-\u007E\u00A0-\u00FF\u2013\u2014\u2018\u2019\u201C\u201D\u2022\u2026]/g, "?");

async function toPicture(blob: Blob): Promise<Picture> {
  const bitmap = await createImageBitmap(blob);
  const scale = Math.min(1, MAX_IMAGE_SIDE / Math.max(bitmap.width, bitmap.height));
  const width = Math.max(1, Math.round(bitmap.width * scale));
  const height = Math.max(1, Math.round(bitmap.height * scale));

  const canvas = document.createElement("canvas");
  canvas.width = width;
  canvas.height = height;

  const context = canvas.getContext("2d");
  if (!context) throw new Error("Canvas indisponível");

  context.fillStyle = "#ffffff";
  context.fillRect(0, 0, width, height);
  context.drawImage(bitmap, 0, 0, width, height);
  bitmap.close();

  return { data: canvas.toDataURL("image/jpeg", 0.85), width, height };
}

class Sheet {
  y = 0;

  constructor(
    private readonly doc: jsPDF,
    private readonly code: string,
  ) {}

  band(title: string) {
    this.doc.setFillColor(...BAND);
    this.doc.rect(0, 0, WIDTH, HEADER, "F");
    this.doc.setFont("helvetica", "bold");
    this.doc.setFontSize(14);
    this.doc.setTextColor(255, 255, 255);
    this.doc.text(clean(title), MARGIN, 13);
    this.doc.setFontSize(9);
    this.doc.setFont("helvetica", "normal");
    this.doc.text(clean(this.code), WIDTH - MARGIN, 13, { align: "right" });
    this.y = HEADER + 10;
  }

  ensure(height: number) {
    if (this.y + height <= HEIGHT - FOOTER) return;
    this.doc.addPage();
    this.band(`INSTRUÇÃO DE TRABALHO - continuação`);
  }

  text(value: string, options: { size?: number; bold?: boolean; color?: Rgb; indent?: number; width?: number; after?: number } = {}) {
    const { size = 10, bold = false, color = INK, indent = 0, width = CONTENT - indent, after = 2 } = options;

    this.doc.setFont("helvetica", bold ? "bold" : "normal");
    this.doc.setFontSize(size);
    this.doc.setTextColor(...color);

    const lineHeight = size * 0.45;
    const lines: string[] = this.doc.splitTextToSize(clean(value), width);

    for (const line of lines) {
      this.ensure(lineHeight);
      this.doc.text(line, MARGIN + indent, this.y + lineHeight * 0.8);
      this.y += lineHeight;
    }

    this.y += after;
  }

  section(title: string, fill: Rgb = PANEL, color: Rgb = INK) {
    this.ensure(14);
    this.doc.setFillColor(...fill);
    this.doc.rect(MARGIN, this.y, CONTENT, 8, "F");
    this.doc.setFont("helvetica", "bold");
    this.doc.setFontSize(10);
    this.doc.setTextColor(...color);
    this.doc.text(clean(title), MARGIN + 3, this.y + 5.4);
    this.y += 12;
  }

  box(value: string, fill: Rgb) {
    const lines: string[] = this.doc.splitTextToSize(clean(value), CONTENT - 8);
    const height = lines.length * 4.6 + 6;

    this.ensure(height);
    this.doc.setFillColor(...fill);
    this.doc.rect(MARGIN, this.y, CONTENT, height, "F");
    this.doc.setFont("helvetica", "normal");
    this.doc.setFontSize(10);
    this.doc.setTextColor(...INK);
    this.doc.text(lines, MARGIN + 4, this.y + 6);
    this.y += height + 4;
  }

  pair(left: [string, string], right: [string, string]) {
    this.ensure(10);
    const half = CONTENT / 2;

    for (const [index, [label, value]] of [left, right].entries()) {
      const x = MARGIN + index * half;
      this.doc.setFont("helvetica", "bold");
      this.doc.setFontSize(8);
      this.doc.setTextColor(...SOFT);
      this.doc.text(clean(label.toUpperCase()), x, this.y + 3);
      this.doc.setFont("helvetica", "normal");
      this.doc.setFontSize(10);
      this.doc.setTextColor(...INK);
      this.doc.text(this.doc.splitTextToSize(clean(value), half - 6)[0] ?? "", x, this.y + 8);
    }

    this.y += 13;
  }

  step(number: number, value: string, picture: Picture | null) {
    const lines: string[] = this.doc.splitTextToSize(clean(value), CONTENT - 14);
    const textHeight = lines.length * 4.6;
    const scale = picture ? Math.min(IMAGE_BOX.width / picture.width, IMAGE_BOX.height / picture.height) : 0;
    const imageWidth = picture ? picture.width * scale : 0;
    const imageHeight = picture ? picture.height * scale : 0;

    this.ensure(Math.min(textHeight + 8, 30));

    this.doc.setFillColor(...BAND);
    this.doc.circle(MARGIN + 4, this.y + 3, 4, "F");
    this.doc.setFont("helvetica", "bold");
    this.doc.setFontSize(9);
    this.doc.setTextColor(255, 255, 255);
    this.doc.text(String(number), MARGIN + 4, this.y + 4.2, { align: "center" });

    this.doc.setFont("helvetica", "normal");
    this.doc.setFontSize(10);
    this.doc.setTextColor(...INK);

    for (const line of lines) {
      this.ensure(4.6);
      this.doc.text(line, MARGIN + 12, this.y + 4);
      this.y += 4.6;
    }

    this.y += 3;

    if (picture) {
      this.ensure(imageHeight + 8);
      this.doc.addImage(picture.data, "JPEG", MARGIN + 12 + (CONTENT - 12 - imageWidth) / 2, this.y, imageWidth, imageHeight);
      this.y += imageHeight + 3;
      this.doc.setFontSize(8);
      this.doc.setTextColor(...SOFT);
      this.doc.text(`Figura - passo ${number}`, MARGIN + 12 + (CONTENT - 12) / 2, this.y + 2, { align: "center" });
      this.y += 6;
    }

    this.y += 4;
  }
}

export async function buildInstructionPdf(input: PdfInput): Promise<Blob> {
  const { jsPDF: Pdf } = await import("jspdf");
  const doc = new Pdf({ orientation: "portrait", unit: "mm", format: "a4" });
  const sheet = new Sheet(doc, input.code);

  sheet.band("INSTRUÇÃO DE TRABALHO");
  sheet.text(input.title, { size: 15, bold: true, after: 5 });

  sheet.pair(["Código", input.code], ["Nº do documento", input.documentNumber]);
  sheet.pair(["Produto", input.product], ["Elaborado por", input.preparedBy]);
  sheet.pair(["Aprovado por", input.approvedBy], ["Emitido em", formatDateTime(new Date().toISOString())]);
  sheet.y += 2;

  sheet.section("EQUIPAMENTO DE PROTEÇÃO INDIVIDUAL (EPI)");
  sheet.text(input.requiredPpe, { indent: 2, after: 6 });

  sheet.section("ESPECIFICAÇÕES DE PROCESSO");
  sheet.text(input.processSpecifications, { indent: 2, after: 6 });

  if (input.notes.trim()) {
    sheet.section("OBSERVAÇÕES IMPORTANTES");
    sheet.box(input.notes, WARNING);
    sheet.y += 2;
  }

  if (input.steps.length > 0) {
    sheet.section("PROCEDIMENTO OPERACIONAL", BAND, [255, 255, 255]);

    for (const [index, step] of input.steps.entries()) {
      const picture = step.image ? await toPicture(step.image) : null;
      sheet.step(index + 1, step.text, picture);
    }
  }

  const total = doc.getNumberOfPages();
  for (let current = 1; current <= total; current++) {
    doc.setPage(current);
    doc.setFont("helvetica", "normal");
    doc.setFontSize(8);
    doc.setTextColor(...SOFT);
    doc.text(clean(`${input.code} - ${input.title}`).slice(0, 80), MARGIN, HEIGHT - 8);
    doc.text(`Página ${current} de ${total}`, WIDTH - MARGIN, HEIGHT - 8, { align: "right" });
  }

  return doc.output("blob");
}
