"use client";

import { useState } from "react";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Typography from "@mui/material/Typography";
import { inkSoft } from "@/lib/tokens";

type Props = { title: string; author: string; date: string; text: string };

export default function JustificationButton({ title, author, date, text }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Button size="small" onClick={() => setOpen(true)}>
        Ver justificativa
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>{title}</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ color: inkSoft, mb: 2 }}>
            {author} · {date}
          </Typography>
          <Typography sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>{text}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Fechar</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
