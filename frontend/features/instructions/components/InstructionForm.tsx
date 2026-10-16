"use client";

import { startTransition, useActionState, useEffect, useRef, useState, type FormEvent } from "react";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import FormControlLabel from "@mui/material/FormControlLabel";
import Radio from "@mui/material/Radio";
import RadioGroup from "@mui/material/RadioGroup";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import { formatBytes } from "@/lib/format";
import type { Result } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import { saveInstruction } from "../actions/instructionActions";
import { FIELD_LIMITS, MAX_IMAGE_BYTES, MAX_PDF_BYTES, MAX_STEPS } from "../limits";
import type { InstructionDetail } from "../types";
import StepsEditor, { type StepDraft } from "./StepsEditor";

type PdfMode = "keep" | "generate" | "upload";

type Props = { instruction?: InstructionDetail; suggestedCode?: string };

const FIELDS = ["title", "documentNumber", "product", "preparedBy", "approvedBy"] as const;

const LABELS: Record<(typeof FIELDS)[number], string> = {
  title: "Título",
  documentNumber: "Nº do documento",
  product: "Produto",
  preparedBy: "Elaborado por",
  approvedBy: "Aprovado por",
};

let draftCounter = 0;
const nextKey = () => `step-${++draftCounter}`;

function draftsOf(instruction?: InstructionDetail): StepDraft[] {
  return (instruction?.steps ?? []).map((step) => ({
    key: nextKey(),
    id: step.id,
    text: step.text,
    file: null,
    preview: step.hasImage ? `/instructions/${instruction!.id}/steps/${step.id}/image` : null,
    hadImage: step.hasImage,
    removeImage: false,
  }));
}

function problemsOf(data: FormData, steps: StepDraft[], mode: PdfMode, pdf: File | null) {
  const problems: string[] = [];
  const value = (key: string) => String(data.get(key) ?? "").trim();

  for (const key of FIELDS) {
    if (!value(key)) problems.push(`Informe o campo "${LABELS[key]}".`);
    else if (value(key).length > FIELD_LIMITS[key]) problems.push(`"${LABELS[key]}" não pode passar de ${FIELD_LIMITS[key]} caracteres.`);
  }

  if (!value("requiredPpe")) problems.push("Informe os EPIs obrigatórios.");
  if (!value("processSpecifications")) problems.push("Informe as especificações de processo.");
  if (steps.length > MAX_STEPS) problems.push(`A instrução pode ter no máximo ${MAX_STEPS} passos.`);
  if (steps.some((step) => !step.text.trim())) problems.push("Todo passo precisa de uma instrução escrita.");
  if (mode === "generate" && steps.length === 0) problems.push("Adicione ao menos um passo para gerar o PDF.");

  if (mode === "upload") {
    if (!pdf) problems.push("Selecione o arquivo PDF.");
    else if (pdf.size > MAX_PDF_BYTES) problems.push(`O PDF passa do tamanho máximo de ${formatBytes(MAX_PDF_BYTES)}.`);
  }

  return problems;
}

async function imageOf(step: StepDraft): Promise<Blob | null> {
  if (step.file) return step.file;
  if (!step.preview || !step.hadImage || step.removeImage) return null;

  const response = await fetch(step.preview);
  if (!response.ok) throw new Error("Imagem do passo indisponível");
  return response.blob();
}

export default function InstructionForm({ instruction, suggestedCode }: Props) {
  const editing = instruction !== undefined;
  const [steps, setSteps] = useState<StepDraft[]>(() => draftsOf(instruction));
  const [mode, setMode] = useState<PdfMode>(editing ? "keep" : "generate");
  const [pdf, setPdf] = useState<File | null>(null);
  const [problems, setProblems] = useState<string[]>([]);
  const [generating, setGenerating] = useState(false);
  const previews = useRef<string[]>([]);

  const [result, formAction, pending] = useActionState(
    (previous: Result | undefined, data: FormData) => saveInstruction(instruction?.id ?? null, previous, data),
    undefined,
  );

  useEffect(
    () => () => {
      for (const url of previews.current) URL.revokeObjectURL(url);
    },
    [],
  );

  function track(url: string) {
    previews.current.push(url);
    return url;
  }

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();

    const data = new FormData(e.currentTarget);
    const found = problemsOf(data, steps, mode, pdf);
    setProblems(found);
    if (found.length > 0) return;

    const code = String(data.get("code") ?? "");

    setGenerating(true);
    try {
      steps.forEach((step, index) => {
        data.append(`steps[${index}].text`, step.text.trim());
        if (step.id !== null) data.append(`steps[${index}].id`, String(step.id));
        if (step.removeImage) data.append(`steps[${index}].removeImage`, "true");
        if (step.file) data.append(`stepImages[${index}]`, step.file, step.file.name);
      });

      if (mode === "generate") {
        const { buildInstructionPdf } = await import("../pdf");
        const value = (key: string) => String(data.get(key) ?? "").trim();

        const blob = await buildInstructionPdf({
          code,
          title: value("title"),
          documentNumber: value("documentNumber"),
          product: value("product"),
          preparedBy: value("preparedBy"),
          approvedBy: value("approvedBy"),
          requiredPpe: value("requiredPpe"),
          processSpecifications: value("processSpecifications"),
          notes: value("notes"),
          steps: await Promise.all(steps.map(async (step) => ({ text: step.text.trim(), image: await imageOf(step) }))),
        });

        data.set("pdf", blob, `${code}.pdf`);
      } else if (mode === "upload" && pdf) {
        data.set("pdf", pdf, pdf.name);
      }
    } catch (error) {
      console.error("[instrução] falha ao montar o PDF", error);
      setProblems(["Não foi possível gerar o PDF. Confira as imagens dos passos e tente de novo."]);
      setGenerating(false);
      return;
    }

    setGenerating(false);
    startTransition(() => formAction(data));
  }

  const busy = generating || pending;

  return (
    <Box component="form" onSubmit={onSubmit} noValidate>
      <Stack spacing={3}>
        {(problems.length > 0 || (result && !result.ok)) && (
          <Stack spacing={1.5}>
            {problems.length > 0 && (
              <Alert severity="error">
                <Box component="ul" sx={{ m: 0, pl: 2.5 }}>
                  {problems.map((problem) => (
                    <li key={problem}>{problem}</li>
                  ))}
                </Box>
              </Alert>
            )}
            {result && !result.ok && <ErrorAlert error={result.error} />}
          </Stack>
        )}

        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Informações
          </Typography>
          <Box sx={{ display: "grid", gap: 2, mt: 1.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" } }}>
            <TextField label="Código" name="code" defaultValue={instruction?.code ?? suggestedCode} slotProps={{ htmlInput: { readOnly: true } }} helperText="Gerado pelo sistema e único por instrução." />
            <TextField label="Nº do documento" name="documentNumber" defaultValue={instruction?.documentNumber} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.documentNumber } }} />
            <TextField label="Título" name="title" defaultValue={instruction?.title} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.title } }} sx={{ gridColumn: { md: "1 / -1" } }} />
            <TextField label="Produto" name="product" defaultValue={instruction?.product} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.product } }} />
            <Box />
            <TextField label="Elaborado por" name="preparedBy" defaultValue={instruction?.preparedBy} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.preparedBy } }} />
            <TextField label="Aprovado por" name="approvedBy" defaultValue={instruction?.approvedBy} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.approvedBy } }} />
          </Box>
        </Card>

        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Especificações
          </Typography>
          <Stack spacing={2} sx={{ mt: 1.5 }}>
            <TextField label="EPI obrigatório" name="requiredPpe" defaultValue={instruction?.requiredPpe} placeholder="Ex.: luvas, óculos de proteção" multiline minRows={2} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.requiredPpe } }} />
            <TextField label="Especificações de processo" name="processSpecifications" defaultValue={instruction?.processSpecifications} multiline minRows={3} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.processSpecifications } }} />
            <TextField label="Observações" name="notes" defaultValue={instruction?.notes ?? ""} multiline minRows={2} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.notes } }} helperText="Aparecem em destaque no PDF gerado." />
          </Stack>
        </Card>

        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Passos ({steps.length})
          </Typography>
          <Typography variant="body2" sx={{ color: inkSoft, mt: 0.5, mb: 2 }}>
            Cada passo tem uma instrução escrita e, se ajudar, uma imagem (PNG, JPEG, GIF ou WebP de até {formatBytes(MAX_IMAGE_BYTES)}).
          </Typography>
          <StepsEditor steps={steps} onChange={setSteps} nextKey={nextKey} track={track} />
        </Card>

        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Documento PDF
          </Typography>
          <RadioGroup value={mode} onChange={(_, next) => setMode(next as PdfMode)} sx={{ mt: 1 }}>
            {editing && (
              <FormControlLabel
                value="keep"
                control={<Radio />}
                label={`Manter o PDF atual (${formatBytes(instruction.pdfSize)})`}
              />
            )}
            <FormControlLabel value="generate" control={<Radio />} label="Gerar o PDF a partir dos passos" />
            <FormControlLabel value="upload" control={<Radio />} label="Enviar um PDF pronto" />
          </RadioGroup>

          {mode === "upload" && (
            <Stack spacing={1} sx={{ mt: 1.5, alignItems: "flex-start" }}>
              <Button component="label" variant="outlined">
                {pdf ? "Trocar arquivo" : "Selecionar PDF"}
                <input type="file" accept="application/pdf,.pdf" hidden onChange={(e) => setPdf(e.target.files?.[0] ?? null)} />
              </Button>
              <Typography variant="body2" sx={{ color: inkSoft }}>
                {pdf ? `${pdf.name} · ${formatBytes(pdf.size)}` : `Somente PDF, até ${formatBytes(MAX_PDF_BYTES)}.`}
              </Typography>
            </Stack>
          )}

          {mode === "generate" && editing && (
            <Typography variant="body2" sx={{ color: inkSoft, mt: 1.5 }}>
              O PDF será gerado de novo com os dados e passos desta tela e substituirá o atual.
            </Typography>
          )}
        </Card>

        <Stack direction="row" spacing={1.5}>
          <Button type="submit" variant="contained" disabled={busy}>
            {generating ? "Gerando PDF..." : pending ? "Salvando..." : editing ? "Salvar alterações" : "Criar instrução"}
          </Button>
          <Button href={editing ? `/instructions/${instruction.id}` : "/instructions"} disabled={busy}>
            Cancelar
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}
