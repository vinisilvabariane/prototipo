import TextField from "@mui/material/TextField";
import { MAX_JUSTIFICATION_LENGTH } from "../labels";

export default function JustificationField({ label = "Justificativa" }: { label?: string }) {
  return (
    <TextField
      label={label}
      name="justification"
      multiline
      minRows={4}
      autoFocus
      slotProps={{ htmlInput: { maxLength: MAX_JUSTIFICATION_LENGTH } }}
      fullWidth
    />
  );
}
