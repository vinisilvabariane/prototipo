"use client";

import { useState, useTransition } from "react";
import PictureAsPdf from "@mui/icons-material/PictureAsPdf";
import Button from "@mui/material/Button";
import ErrorAlert from "@/components/ui/ErrorAlert";
import { failure, type AppError } from "@/lib/result";
import { loadStepMessages } from "../actions/workflowActions";
import type { WorkflowDetail, WorkflowMessage } from "../types";

export default function WorkflowPdfButton({ workflow }: { workflow: WorkflowDetail }) {
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<AppError>();

  function generate() {
    setError(undefined);

    startTransition(async () => {
      const messages = new Map<number, WorkflowMessage[]>();

      for (const step of workflow.steps.filter((candidate) => candidate.action === "rejection")) {
        const result = await loadStepMessages(workflow.id, step.id);
        if (!result.ok) {
          setError(result.error);
          return;
        }
        messages.set(step.id, result.value);
      }

      try {
        const { saveWorkflowPdf } = await import("../pdf");
        await saveWorkflowPdf(workflow, messages);
      } catch (e) {
        console.error("[pdf] falha ao gerar o relatório", e);
        setError(failure("unexpected", { title: "Não foi possível gerar o PDF" }).error);
      }
    });
  }

  return (
    <>
      <Button variant="outlined" startIcon={<PictureAsPdf />} onClick={generate} disabled={pending}>
        {pending ? "Gerando..." : "Gerar PDF"}
      </Button>
      {error && <ErrorAlert error={error} />}
    </>
  );
}
