import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import { FIELD_LIMITS } from "../labels";
import { formatClock } from "../format";
import type { ProductionLine, ProductionProduct, ProductionShift } from "../types";

const limit = (max: number) => ({ htmlInput: { maxLength: max } });

function ActiveSwitch({ checked }: { checked: boolean }) {
  return <FormControlLabel control={<Switch name="active" defaultChecked={checked} />} label="Ativo" />;
}

export function LineFields({ line }: { line?: ProductionLine }) {
  return (
    <>
      <TextField label="Nome da linha" name="name" defaultValue={line?.name} slotProps={limit(FIELD_LIMITS.lineName)} required fullWidth autoFocus />
      <ActiveSwitch checked={line?.active ?? true} />
    </>
  );
}

export function ShiftFields({ shift }: { shift?: ProductionShift }) {
  return (
    <>
      <TextField label="Nome do turno" name="name" defaultValue={shift?.name} slotProps={limit(FIELD_LIMITS.shiftName)} required fullWidth autoFocus />
      <Stack direction="row" spacing={2}>
        <TextField label="Início" name="startTime" type="time" defaultValue={shift ? formatClock(shift.startTime) : "06:00"} slotProps={{ inputLabel: { shrink: true } }} required fullWidth />
        <TextField label="Fim" name="endTime" type="time" defaultValue={shift ? formatClock(shift.endTime) : "14:00"} slotProps={{ inputLabel: { shrink: true } }} required fullWidth />
      </Stack>
      <ActiveSwitch checked={shift?.active ?? true} />
    </>
  );
}

export function ProductFields({ product }: { product?: ProductionProduct }) {
  return (
    <>
      <TextField label="Código" name="code" defaultValue={product?.code} slotProps={limit(FIELD_LIMITS.productCode)} required fullWidth autoFocus />
      <TextField label="Nome do produto" name="name" defaultValue={product?.name} slotProps={limit(FIELD_LIMITS.productName)} required fullWidth />
      <ActiveSwitch checked={product?.active ?? true} />
    </>
  );
}
