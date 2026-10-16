"use client";

import { useState, useTransition } from "react";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { createRun } from "../actions/runActions";

type Props = { layoutId: number; label?: string; variant?: "contained" | "outlined" | "text"; size?: "small" | "medium" };

export default function StartRunButton({ layoutId, label = "Preparar produção", variant = "contained", size = "medium" }: Props) {
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  function start() {
    setError(undefined);

    startTransition(async () => {
      const result = await createRun(layoutId);
      if (result && !result.ok) setError(result.error);
    });
  }

  return (
    <Stack spacing={1}>
      <Button variant={variant} size={size} onClick={start} disabled={pending}>
        {pending ? "Preparando..." : label}
      </Button>
      {error && <ErrorAlert error={error} />}
    </Stack>
  );
}
