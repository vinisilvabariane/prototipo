import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import InstructionForm from "@/features/instructions/components/InstructionForm";
import { instructionService } from "@/features/instructions/services/InstructionService";

export const metadata: Metadata = { title: "Editar instrução de trabalho" };

export default async function EditInstructionPage({ params }: PageProps<"/instructions/[id]/edit">) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id <= 0) notFound();

  const result = await instructionService.getById(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Editar instrução" error={result.error} retryHref={`/instructions/${id}/edit`} />;
  if (!result.value.canManage) redirect(`/instructions/${id}`);

  return (
    <Stack spacing={2.5} sx={{ maxWidth: 960 }}>
      <PageHeader title="Editar instrução de trabalho" back={{ href: `/instructions/${id}`, label: result.value.code }} />
      <InstructionForm instruction={result.value} />
    </Stack>
  );
}
