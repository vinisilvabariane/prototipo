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
  maxWidth?: "xs" | "sm" | "md";
  submitLabel?: string;
  color?: "primary" | "error" | "success";
  variant?: "contained" | "outlined" | "text";
};

export default function FormDialog({ title, trigger, action, children, compact, maxWidth = "xs", submitLabel = "Salvar", color = "primary", variant }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Button
        variant={variant ?? (compact ? "text" : "contained")}
        size={compact ? "small" : "medium"}
        color={color}
        onClick={() => setOpen(true)}
      >
        {trigger}
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth={maxWidth}>
        <DialogForm title={title} action={action} submitLabel={submitLabel} color={color} onClose={() => setOpen(false)}>
          {children}
        </DialogForm>
      </Dialog>
    </>
  );
}

type DialogFormProps = {
  title: string;
  action: FormAction;
  submitLabel: string;
  color: "primary" | "error" | "success";
  onClose: () => void;
  children: ReactNode;
};

function DialogForm({ title, action, submitLabel, color, onClose, children }: DialogFormProps) {
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
        <Button type="submit" variant="contained" color={color} disabled={pending}>
          {pending ? "Salvando..." : submitLabel}
        </Button>
      </DialogActions>
    </form>
  );
}
