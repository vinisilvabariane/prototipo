"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { openTicket } from "../actions/ticketActions";
import FormRenderer from "../form/FormRenderer";
import { firstProblem, visibleFields } from "../schema";
import type { Answers, Location, SectorDetail } from "../types";

type Props = { sector: SectorDetail; locations: Location[] };

export default function OpenTicketForm({ sector, locations }: Props) {
  const router = useRouter();
  const [answers, setAnswers] = useState<Answers>({});
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  function submit() {
    const problem = firstProblem(sector.form.fields, answers);

    if (problem) {
      setError({ code: "badRequest", status: null, title: "Falta responder", messages: [problem] });
      return;
    }

    setError(undefined);
    const sent: Answers = {};
    for (const field of visibleFields(sector.form.fields, answers)) {
      for (const [key, value] of Object.entries(answers)) if ((key === field.id || key.startsWith(`${field.id}_`)) && value !== "") sent[key] = value;
    }

    startTransition(async () => {
      const result = await openTicket(sector.id, sent);

      if (!result.ok) {
        setError(result.error);
        return;
      }

      router.push(`/helpdesk/tickets/${result.value.id}`);
    });
  }

  return (
    <Card sx={{ p: { xs: 2, md: 3 }, borderTop: `4px solid ${sector.colorHex}` }}>
      <Stack spacing={3}>
        {error && <ErrorAlert error={error} />}
        <FormRenderer fields={sector.form.fields} answers={answers} onChange={setAnswers} locations={locations} disabled={pending} />
        <Box sx={{ display: "flex", justifyContent: "flex-end", gap: 1 }}>
          <Button href="/helpdesk" color="inherit" disabled={pending}>
            Cancelar
          </Button>
          <Button variant="contained" onClick={submit} disabled={pending}>
            {pending ? "Abrindo..." : "Abrir chamado"}
          </Button>
        </Box>
      </Stack>
    </Card>
  );
}
