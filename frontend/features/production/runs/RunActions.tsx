"use client";

import { useState, useTransition } from "react";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import Snackbar from "@mui/material/Snackbar";
import Stack from "@mui/material/Stack";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError, Result } from "@/lib/result";
import { cancelRun, closeRun, copyPreviousAllocations, releaseAllPosts, startRun } from "../actions/runActions";
import type { RunStatus } from "../types";

type Props = { runId: number; status: RunStatus; canSupervise: boolean; hasAllocations: boolean };

type Confirm = { title: string; text: string; label: string; color: "primary" | "error"; run: () => Promise<Result<unknown>> };

export default function RunActions({ runId, status, canSupervise, hasAllocations }: Props) {
  const [error, setError] = useState<AppError>();
  const [notice, setNotice] = useState<string>();
  const [confirm, setConfirm] = useState<Confirm | null>(null);
  const [pending, startTransition] = useTransition();

  if (!canSupervise || status === "closed") return null;

  function perform(action: () => Promise<Result<unknown>>, onDone?: (value: unknown) => void) {
    setError(undefined);

    startTransition(async () => {
      const result = await action();

      if (!result.ok) {
        setError(result.error);
        return;
      }

      onDone?.(result.value);
    });
  }

  function copyPrevious() {
    perform(
      () => copyPreviousAllocations(runId),
      (value) => {
        const { allocated, skipped } = value as { allocated: number; skipped: number };
        setNotice(`${allocated} pessoa(s) alocada(s)${skipped > 0 ? `, ${skipped} não puderam ser repetidas` : ""}.`);
      },
    );
  }

  return (
    <Stack spacing={1.5}>
      <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", rowGap: 1 }}>
        {status === "preparing" && (
          <>
            <Button variant="outlined" onClick={copyPrevious} disabled={pending}>
              Repetir alocação anterior
            </Button>
            <Button
              color="inherit"
              onClick={() => setConfirm({ title: "Liberar todos os postos?", text: "Todas as pessoas saem dos postos. Você pode alocar de novo em seguida.", label: "Liberar todos", color: "primary", run: () => releaseAllPosts(runId) })}
              disabled={pending || !hasAllocations}
            >
              Liberar todos
            </Button>
            <Button variant="contained" onClick={() => perform(() => startRun(runId))} disabled={pending}>
              Iniciar produção
            </Button>
            <Button
              color="error"
              onClick={() => setConfirm({ title: "Cancelar esta produção?", text: "A produção em preparo é apagada, junto com as alocações feitas.", label: "Cancelar produção", color: "error", run: () => cancelRun(runId) })}
              disabled={pending}
            >
              Cancelar
            </Button>
          </>
        )}
        {status === "running" && (
          <>
            <Button
              color="inherit"
              onClick={() => setConfirm({ title: "Liberar todos os postos?", text: "Todas as pessoas saem dos postos, mas a produção segue em andamento.", label: "Liberar todos", color: "primary", run: () => releaseAllPosts(runId) })}
              disabled={pending || !hasAllocations}
            >
              Liberar todos
            </Button>
            <Button
              variant="contained"
              color="error"
              onClick={() => setConfirm({ title: "Encerrar a produção?", text: "Todos saem dos postos e a produtividade fica fechada. Depois disso não dá mais para lançar produção.", label: "Encerrar produção", color: "error", run: () => closeRun(runId) })}
              disabled={pending}
            >
              Encerrar produção
            </Button>
          </>
        )}
      </Stack>

      {error && <ErrorAlert error={error} />}

      <Dialog open={confirm !== null} onClose={() => setConfirm(null)} fullWidth maxWidth="xs">
        <DialogTitle>{confirm?.title}</DialogTitle>
        <DialogContent>
          <DialogContentText>{confirm?.text}</DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirm(null)}>Voltar</Button>
          <Button
            variant="contained"
            color={confirm?.color}
            disabled={pending}
            onClick={() => {
              const action = confirm?.run;
              setConfirm(null);
              if (action) perform(action);
            }}
          >
            {confirm?.label}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={Boolean(notice)} autoHideDuration={4000} onClose={() => setNotice(undefined)} message={notice} />
    </Stack>
  );
}
