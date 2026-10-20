import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import InstructionForm from "@/features/instructions/components/InstructionForm";
import { instructionService } from "@/features/instructions/services/InstructionService";

export const metadata: Metadata = { title: "Nova instrução de trabalho" };

export default async function NewInstructionPage() {
  const code = await instructionService.nextCode();
  if (!code.ok) return <PageError title="Nova instrução de trabalho" error={code.error} retryHref="/instructions/new" />;

  return (
    <Stack spacing={2.5} sx={{ maxWidth: 960 }}>
      <PageHeader title="Nova instrução de trabalho" back={{ href: "/instructions", label: "Instruções" }} />
      <InstructionForm suggestedCode={code.value.code} />
    </Stack>
  );
}
