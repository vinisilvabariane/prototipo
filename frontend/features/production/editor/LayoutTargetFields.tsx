import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import type { ProductionLine, ProductionProduct, ProductionShift } from "../types";

type Props = {
  lines: ProductionLine[];
  products: ProductionProduct[];
  shifts: ProductionShift[];
  defaults?: { lineId?: number; productId?: number; shiftId?: number };
};

export default function LayoutTargetFields({ lines, products, shifts, defaults }: Props) {
  return (
    <>
      <TextField select label="Linha" name="lineId" defaultValue={defaults?.lineId ?? ""} required fullWidth>
        {lines.map((line) => (
          <MenuItem key={line.id} value={line.id}>
            {line.name}
          </MenuItem>
        ))}
      </TextField>
      <TextField select label="Produto" name="productId" defaultValue={defaults?.productId ?? ""} required fullWidth>
        {products.map((product) => (
          <MenuItem key={product.id} value={product.id}>
            {product.code} · {product.name}
          </MenuItem>
        ))}
      </TextField>
      <TextField select label="Turno" name="shiftId" defaultValue={defaults?.shiftId ?? ""} required fullWidth>
        {shifts.map((shift) => (
          <MenuItem key={shift.id} value={shift.id}>
            {shift.name}
          </MenuItem>
        ))}
      </TextField>
    </>
  );
}
