"use client";

import { startTransition, useActionState, useState, type FormEvent, type ReactNode } from "react";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Stack from "@mui/material/Stack";
import type { Result } from "@/lib/result";
import ErrorAlert from "./ErrorAlert";

type FormAction = (prev: Result | undefined, formData: FormData) => Promise<Result>;

type Props = {
  title: string;
  trigger: string;
  action: FormAction;
  children: ReactNode;
  compact?: boolean;
};

export default function FormDialog({ title, trigger, action, children, compact }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Button variant={compact ? "text" : "contained"} size={compact ? "small" : "medium"} onClick={() => setOpen(true)}>
        {trigger}
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="xs">
        <DialogForm title={title} action={action} onClose={() => setOpen(false)}>
          {children}
        </DialogForm>
      </Dialog>
    </>
  );
}

function DialogForm({ title, action, onClose, children }: { title: string; action: FormAction; onClose: () => void; children: ReactNode }) {
  const [result, formAction, pending] = useActionState(async (prev: Result | undefined, formData: FormData) => {
    const next = await action(prev, formData);
    if (next.ok) onClose();
    return next;
  }, undefined);

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    startTransition(() => formAction(formData));
  }

  return (
    <form onSubmit={handleSubmit}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {result && !result.ok && <ErrorAlert error={result.error} />}
          {children}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancelar</Button>
        <Button type="submit" variant="contained" disabled={pending}>
          {pending ? "Salvando..." : "Salvar"}
        </Button>
      </DialogActions>
    </form>
  );
}
