"use client";

import { useState, useTransition } from "react";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError, Result } from "@/lib/result";
import { rejectTicket, startTicket } from "../actions/ticketActions";

type Props = { id: number; canStart: boolean; canReject: boolean; compact?: boolean };

export default function TicketActions({ id, canStart, canReject, compact }: Props) {
  const [error, setError] = useState<AppError>();
  const [rejecting, setRejecting] = useState(false);
  const [reason, setReason] = useState("");
  const [pending, startTransition] = useTransition();

  if (!canStart && !canReject) return null;

  function run(action: () => Promise<Result>, done?: () => void) {
    setError(undefined);

    startTransition(async () => {
      const result = await action();

      if (!result.ok) {
        setError(result.error);
        return;
      }

      done?.();
    });
  }

  return (
    <Stack spacing={1}>
      {error && !rejecting && <ErrorAlert error={error} />}
      <Stack direction="row" spacing={1}>
        {canStart && (
          <Button variant="contained" size={compact ? "small" : "medium"} disabled={pending} onClick={() => run(() => startTicket(id))}>
            Assumir
          </Button>
        )}
        {canReject && (
          <Button color="inherit" size={compact ? "small" : "medium"} disabled={pending} onClick={() => setRejecting(true)}>
            Recusar
          </Button>
        )}
      </Stack>
      <Dialog open={rejecting} onClose={() => setRejecting(false)} fullWidth maxWidth="xs">
        <DialogTitle>Recusar chamado</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {error && <ErrorAlert error={error} />}
            <TextField
              label="Motivo da recusa"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              multiline
              minRows={3}
              required
              autoFocus
              fullWidth
              helperText="Quem abriu vê esse motivo. Chamado recusado não conta como parada."
              slotProps={{ htmlInput: { maxLength: 500 } }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejecting(false)}>Cancelar</Button>
          <Button color="error" variant="contained" disabled={pending || !reason.trim()} onClick={() => run(() => rejectTicket(id, reason), () => setRejecting(false))}>
            {pending ? "Recusando..." : "Recusar chamado"}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
