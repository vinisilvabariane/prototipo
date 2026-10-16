"use client";

import { useMemo, useState, useTransition } from "react";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import { recordOutput } from "../actions/runActions";
import { formatHour } from "../format";

type Props = {
  runId: number;
  postId: number;
  postName: string;
  startedAt: string;
  open: boolean;
  onClose: () => void;
};

const HOUR = 3_600_000;
const MAX_SLOTS = 24;

function slotsSince(startedAt: string) {
  const first = Math.floor(new Date(startedAt).getTime() / HOUR) * HOUR;
  const current = Math.floor(Date.now() / HOUR) * HOUR;
  const slots: number[] = [];

  for (let slot = current; slot >= first && slots.length < MAX_SLOTS; slot -= HOUR) slots.push(slot);

  return slots;
}

export default function RecordOutputDialog({ runId, postId, postName, startedAt, open, onClose }: Props) {
  const slots = useMemo(() => (open ? slotsSince(startedAt) : []), [open, startedAt]);
  const [slot, setSlot] = useState<number | null>(null);
  const [quantity, setQuantity] = useState("");
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  const chosen = slot ?? slots[0] ?? 0;

  function submit() {
    setError(undefined);

    startTransition(async () => {
      const result = await recordOutput(runId, postId, Number(quantity), new Date(chosen).toISOString());

      if (!result.ok) {
        setError(result.error);
        return;
      }

      setQuantity("");
      setSlot(null);
      onClose();
    });
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Lançar produção de {postName}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {error && <ErrorAlert error={error} />}
          <TextField select label="Hora" value={chosen} onChange={(event) => setSlot(Number(event.target.value))} fullWidth>
            {slots.map((value) => (
              <MenuItem key={value} value={value}>
                {formatHour(new Date(value).toISOString())} às {formatHour(new Date(value + HOUR).toISOString())}
                {value === slots[0] && " (hora atual)"}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Quantidade produzida"
            type="number"
            value={quantity}
            onChange={(event) => setQuantity(event.target.value)}
            slotProps={{ htmlInput: { min: 0, step: 1 } }}
            autoFocus
            fullWidth
          />
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Lançar de novo na mesma hora substitui o valor anterior. A produção conta para quem estava no posto naquela hora.
          </Typography>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancelar</Button>
        <Button variant="contained" onClick={submit} disabled={pending || quantity === ""}>
          {pending ? "Lançando..." : "Lançar"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
