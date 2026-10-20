"use client";

import { useState, useTransition } from "react";
import DeleteOutlined from "@mui/icons-material/DeleteOutlined";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import type { AppError, Result } from "@/lib/result";
import ErrorAlert from "./ErrorAlert";

type Props = {
  action: () => Promise<Result>;
  name: string;
  iconOnly?: boolean;
};

export default function ConfirmDeleteButton({ action, name, iconOnly }: Props) {
  const [open, setOpen] = useState(false);
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  function confirm() {
    startTransition(async () => {
      const result = await action();
      if (result.ok) setOpen(false);
      else setError(result.error);
    });
  }

  function show() {
    setError(undefined);
    setOpen(true);
  }

  return (
    <>
      {iconOnly ? (
        <Tooltip title="Excluir">
          <IconButton size="small" color="error" aria-label={`Excluir ${name}`} onClick={show}>
            <DeleteOutlined fontSize="small" />
          </IconButton>
        </Tooltip>
      ) : (
        <Button size="small" color="error" onClick={show}>
          Excluir
        </Button>
      )}
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="xs">
        <DialogTitle>Excluir {name}?</DialogTitle>
        <DialogContent>
          {error ? <ErrorAlert error={error} /> : <DialogContentText>Essa ação não pode ser desfeita.</DialogContentText>}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancelar</Button>
          <Button color="error" variant="contained" onClick={confirm} disabled={pending}>
            {pending ? "Excluindo..." : "Excluir"}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
