import type { Metadata } from "next";
import { notFound } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import PageError from "@/components/ui/PageError";
import { deleteInstruction } from "@/features/instructions/actions/instructionActions";
import { instructionService } from "@/features/instructions/services/InstructionService";
import { formatBytes, formatDateTime } from "@/lib/format";
import { inkSoft, line } from "@/lib/tokens";
import Field from "@/components/ui/Field";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/instructions/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Instrução não encontrada" };

  const result = await instructionService.getById(id);
  if (result.ok) return { title: `${result.value.code} · ${result.value.title}` };
  return { title: result.error.code === "notFound" ? "Instrução não encontrada" : "Instrução de trabalho" };
}

function TextCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Card sx={{ p: 3 }}>
      <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
        {title}
      </Typography>
      <Typography sx={{ mt: 1.5, whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>{children}</Typography>
    </Card>
  );
}

export default async function InstructionPage({ params }: PageProps<"/instructions/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const result = await instructionService.getById(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Instrução de trabalho" error={result.error} retryHref={`/instructions/${id}`} />;

  const instruction = result.value;

  return (
    <Stack spacing={4}>
      <Box>
        <Button href="/instructions" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Instruções
        </Button>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ mt: 1, justifyContent: "space-between", alignItems: { md: "flex-start" } }}>
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="overline" component="p" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
              {instruction.code}
            </Typography>
            <Typography variant="h4" component="h1" sx={{ overflowWrap: "anywhere" }}>
              {instruction.title}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", rowGap: 1, alignItems: "flex-start" }}>
            <Button variant="contained" href={`/instructions/${id}/pdf`} target="_blank" rel="noopener">
              Abrir PDF
            </Button>
            <Button variant="outlined" href={`/instructions/${id}/pdf?download=1`}>
              Baixar PDF
            </Button>
            {instruction.canManage && (
              <>
                <Button variant="outlined" href={`/instructions/${id}/edit`}>
                  Editar
                </Button>
                <ConfirmDeleteButton name={instruction.code} action={deleteInstruction.bind(null, id)} />
              </>
            )}
          </Stack>
        </Stack>
      </Box>

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 1fr) minmax(0, 1fr)" }, alignItems: "start" }}>
        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Informações
          </Typography>
          <Box sx={{ mt: 1 }}>
            <Field label="Nº do documento">{instruction.documentNumber}</Field>
            <Field label="Produto">{instruction.product}</Field>
            <Field label="Elaborado por">{instruction.preparedBy}</Field>
            <Field label="Aprovado por">{instruction.approvedBy}</Field>
            <Field label="Criada por">
              {instruction.createdBy} · {formatDateTime(instruction.createdAt)}
            </Field>
            <Field label="Atualizada por">
              {instruction.updatedBy} · {formatDateTime(instruction.updatedAt)}
            </Field>
            <Field label="PDF">{formatBytes(instruction.pdfSize)}</Field>
          </Box>
        </Card>

        <Stack spacing={3}>
          <TextCard title="EPI obrigatório">{instruction.requiredPpe}</TextCard>
          <TextCard title="Especificações de processo">{instruction.processSpecifications}</TextCard>
          {instruction.notes && <TextCard title="Observações">{instruction.notes}</TextCard>}
        </Stack>
      </Box>

      {instruction.steps.length > 0 && (
        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Procedimento ({instruction.steps.length} {instruction.steps.length === 1 ? "passo" : "passos"})
          </Typography>
          <Box component="ol" sx={{ listStyle: "none", m: 0, mt: 2, p: 0, display: "grid", gap: 3 }}>
            {instruction.steps.map((step) => (
              <Box key={step.id} component="li" sx={{ display: "flex", gap: 2 }}>
                <Box
                  aria-hidden
                  sx={{ flexShrink: 0, width: 32, height: 32, borderRadius: "50%", display: "grid", placeItems: "center", fontWeight: 600, fontVariantNumeric: "tabular-nums", bgcolor: "text.primary", color: "background.paper" }}
                >
                  {step.position}
                </Box>
                <Box sx={{ minWidth: 0 }}>
                  <Typography sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>{step.text}</Typography>
                  {step.hasImage && (
                    <Box
                      component="img"
                      src={`/instructions/${id}/steps/${step.id}/image`}
                      alt={`Imagem do passo ${step.position}`}
                      loading="lazy"
                      sx={{ mt: 1.5, maxWidth: "100%", maxHeight: 360, borderRadius: "10px", border: `1px solid ${line}`, display: "block" }}
                    />
                  )}
                </Box>
              </Box>
            ))}
          </Box>
        </Card>
      )}

      <Card sx={{ p: 3 }}>
        <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
          Documento
        </Typography>
        <Box
          component="iframe"
          src={`/instructions/${id}/pdf`}
          title={`PDF da instrução ${instruction.code}`}
          sx={{ mt: 1.5, width: "100%", height: "75vh", minHeight: 420, border: `1px solid ${line}`, borderRadius: "10px", bgcolor: "background.default" }}
        />
      </Card>
    </Stack>
  );
}
