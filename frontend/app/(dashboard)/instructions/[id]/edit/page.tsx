import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import InstructionForm from "@/features/instructions/components/InstructionForm";
import { instructionService } from "@/features/instructions/services/InstructionService";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = { title: "Editar instrução de trabalho" };

export default async function EditInstructionPage({ params }: PageProps<"/instructions/[id]/edit">) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id <= 0) notFound();

  const result = await instructionService.getById(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Editar instrução" error={result.error} retryHref={`/instructions/${id}/edit`} />;
  if (!result.value.canManage) redirect(`/instructions/${id}`);

  return (
    <Box sx={{ maxWidth: 960 }}>
      <Button href={`/instructions/${id}`} size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
        {result.value.code}
      </Button>
      <Typography variant="h4" component="h1" sx={{ mt: 1, mb: 3 }}>
        Editar instrução de trabalho
      </Typography>
      <InstructionForm instruction={result.value} />
    </Box>
  );
}
