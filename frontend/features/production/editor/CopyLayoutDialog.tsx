"use client";

import { useMemo, useState, useTransition } from "react";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import ListItemText from "@mui/material/ListItemText";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import { copyLayout } from "../actions/layoutActions";
import type { LayoutTarget, ProductionLine, ProductionProduct, ProductionShift } from "../types";

type Props = {
  layoutId: number;
  origin: LayoutTarget;
  lines: ProductionLine[];
  products: ProductionProduct[];
  shifts: ProductionShift[];
  blockedReason: string | null;
};

type Option = { id: number; label: string };

function MultiSelect({ label, options, value, onChange }: { label: string; options: Option[]; value: number[]; onChange: (value: number[]) => void }) {
  return (
    <TextField
      select
      label={label}
      value={value}
      onChange={(event) => onChange(event.target.value as unknown as number[])}
      slotProps={{ select: { multiple: true, renderValue: (selected) => options.filter((option) => (selected as number[]).includes(option.id)).map((option) => option.label).join(", ") } }}
      fullWidth
    >
      {options.map((option) => (
        <MenuItem key={option.id} value={option.id}>
          <Checkbox checked={value.includes(option.id)} size="small" />
          <ListItemText primary={option.label} />
        </MenuItem>
      ))}
    </TextField>
  );
}

export default function CopyLayoutDialog({ layoutId, origin, lines, products, shifts, blockedReason }: Props) {
  const [open, setOpen] = useState(false);
  const [lineIds, setLineIds] = useState<number[]>([]);
  const [productIds, setProductIds] = useState<number[]>([]);
  const [shiftIds, setShiftIds] = useState<number[]>([]);
  const [error, setError] = useState<AppError>();
  const [message, setMessage] = useState<string>();
  const [pending, startTransition] = useTransition();

  const targets = useMemo<LayoutTarget[]>(
    () =>
      lineIds
        .flatMap((lineId) => productIds.flatMap((productId) => shiftIds.map((shiftId) => ({ lineId, productId, shiftId }))))
        .filter((target) => !(target.lineId === origin.lineId && target.productId === origin.productId && target.shiftId === origin.shiftId)),
    [lineIds, productIds, shiftIds, origin],
  );

  function submit() {
    setError(undefined);
    setMessage(undefined);

    startTransition(async () => {
      const result = await copyLayout(layoutId, targets);

      if (!result.ok) {
        setError(result.error);
        return;
      }

      const { created, updated } = result.value;
      setMessage(`${created} criado(s) e ${updated} atualizado(s).`);
    });
  }

  return (
    <>
      <Button variant="outlined" onClick={() => setOpen(true)} disabled={Boolean(blockedReason)} title={blockedReason ?? undefined}>
        Copiar para…
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Copiar este layout</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography sx={{ color: inkSoft }}>Escolha onde repetir o mapa. Cada combinação de linha, produto e turno recebe uma cópia; as que já têm layout são sobrescritas.</Typography>
            {error && <ErrorAlert error={error} />}
            {message && <Alert severity="success">{message}</Alert>}
            <MultiSelect label="Linhas" options={lines.map((line) => ({ id: line.id, label: line.name }))} value={lineIds} onChange={setLineIds} />
            <MultiSelect label="Produtos" options={products.map((product) => ({ id: product.id, label: `${product.code} · ${product.name}` }))} value={productIds} onChange={setProductIds} />
            <MultiSelect label="Turnos" options={shifts.map((shift) => ({ id: shift.id, label: shift.name }))} value={shiftIds} onChange={setShiftIds} />
            <Typography variant="body2" sx={{ color: inkSoft }}>
              {targets.length === 0 ? "Nenhum destino selecionado." : `${targets.length} destino(s) na cópia.`}
            </Typography>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Fechar</Button>
          <Button variant="contained" onClick={submit} disabled={pending || targets.length === 0}>
            {pending ? "Copiando..." : "Copiar layout"}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
