import type { Metadata } from "next";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import InstructionForm from "@/features/instructions/components/InstructionForm";
import { instructionService } from "@/features/instructions/services/InstructionService";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = { title: "Nova instrução de trabalho" };

export default async function NewInstructionPage() {
  const code = await instructionService.nextCode();
  if (!code.ok) return <PageError title="Nova instrução de trabalho" error={code.error} retryHref="/instructions/new" />;

  return (
    <Box sx={{ maxWidth: 960 }}>
      <Button href="/instructions" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
        Instruções
      </Button>
      <Typography variant="h4" component="h1" sx={{ mt: 1, mb: 3 }}>
        Nova instrução de trabalho
      </Typography>
      <InstructionForm suggestedCode={code.value.code} />
    </Box>
  );
}
