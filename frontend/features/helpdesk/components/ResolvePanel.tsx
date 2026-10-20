"use client";

import { useState, useTransition } from "react";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { resolveTicket } from "../actions/ticketActions";
import FormRenderer from "../form/FormRenderer";
import { firstProblem } from "../schema";
import type { Answers, HelpdeskForm } from "../types";

type Props = { id: number; form: HelpdeskForm };

export default function ResolvePanel({ id, form }: Props) {
  const [answers, setAnswers] = useState<Answers>({});
  const [note, setNote] = useState("");
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  function submit() {
    const problem = firstProblem(form.fields, answers);

    if (problem) {
      setError({ code: "badRequest", status: null, title: "Falta responder", messages: [problem] });
      return;
    }

    setError(undefined);

    startTransition(async () => {
      const result = await resolveTicket(id, answers, note);
      if (!result.ok) setError(result.error);
    });
  }

  return (
    <Stack spacing={2.5}>
      {error && <ErrorAlert error={error} />}
      <FormRenderer fields={form.fields} answers={answers} onChange={setAnswers} disabled={pending} />
      <TextField label="Observação (opcional)" value={note} onChange={(event) => setNote(event.target.value)} multiline minRows={2} fullWidth slotProps={{ htmlInput: { maxLength: 1000 } }} />
      <Stack direction="row" sx={{ justifyContent: "flex-end" }}>
        <Button variant="contained" color="success" onClick={submit} disabled={pending}>
          {pending ? "Finalizando..." : "Finalizar chamado"}
        </Button>
      </Stack>
    </Stack>
  );
}
