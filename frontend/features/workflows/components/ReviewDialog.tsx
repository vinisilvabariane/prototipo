"use client";

import { startTransition, useActionState, useState, type FormEvent } from "react";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import Stack from "@mui/material/Stack";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { Result } from "@/lib/result";
import { reviewRejection } from "../actions/workflowActions";
import JustificationField from "./JustificationField";

type Props = { workflowId: number; stepId: number; role: "creator" | "assignee" };

const COPY = {
  creator: {
    trigger: "Analisar rejeição",
    title: "Analisar rejeição",
    text: "Responda a quem rejeitou para pedir uma nova análise, ou aceite a rejeição para encerrar o workflow como rejeitado.",
    giveBack: "Devolver rejeição",
    decide: { label: "Aceitar rejeição", intent: "accept", color: "error" },
  },
  assignee: {
    trigger: "Revisar minha rejeição",
    title: "Revisar minha rejeição",
    text: "O criador respondeu à sua rejeição. Aprove o workflow ou devolva a rejeição ao criador com a sua resposta.",
    giveBack: "Devolver rejeição",
    decide: { label: "Aprovar workflow", intent: "approve", color: "primary" },
  },
} as const;

export default function ReviewDialog({ workflowId, stepId, role }: Props) {
  const copy = COPY[role];
  const [open, setOpen] = useState(false);

  const [result, formAction, pending] = useActionState(async (prev: Result | undefined, formData: FormData) => {
    const next = await reviewRejection(workflowId, stepId, role, prev, formData);
    if (next.ok) setOpen(false);
    return next;
  }, undefined);

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const submitter = (e.nativeEvent as SubmitEvent).submitter as HTMLElement | null;
    const formData = new FormData(e.currentTarget, submitter);
    startTransition(() => formAction(formData));
  }

  return (
    <>
      <Button size="small" variant="outlined" onClick={() => setOpen(true)}>
        {copy.trigger}
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="xs">
        <form onSubmit={handleSubmit}>
          <DialogTitle>{copy.title}</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ pt: 1 }}>
              <DialogContentText>{copy.text}</DialogContentText>
              {result && !result.ok && <ErrorAlert error={result.error} />}
              <JustificationField />
            </Stack>
          </DialogContent>
          <DialogActions sx={{ flexWrap: "wrap", gap: 1 }}>
            <Button onClick={() => setOpen(false)}>Cancelar</Button>
            <Button type="submit" name="intent" value="return" variant="outlined" disabled={pending}>
              {copy.giveBack}
            </Button>
            <Button type="submit" name="intent" value={copy.decide.intent} variant="contained" color={copy.decide.color} disabled={pending}>
              {copy.decide.label}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  );
}
