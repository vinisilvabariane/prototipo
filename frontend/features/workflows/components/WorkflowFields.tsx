import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import {
  CATEGORIES,
  CATEGORY_LABELS,
  MAX_DESCRIPTION_LENGTH,
  MAX_TITLE_LENGTH,
  PRIORITIES,
  PRIORITY_LABELS,
} from "../labels";
import type { WorkflowSummary } from "../types";

export default function WorkflowFields({ workflow }: { workflow?: WorkflowSummary }) {
  return (
    <>
      <TextField label="Título" name="title" defaultValue={workflow?.title} slotProps={{ htmlInput: { maxLength: MAX_TITLE_LENGTH } }} fullWidth />
      <TextField
        label="Descrição"
        name="description"
        defaultValue={workflow?.description}
        multiline
        minRows={3}
        slotProps={{ htmlInput: { maxLength: MAX_DESCRIPTION_LENGTH } }}
        fullWidth
      />
      <TextField select label="Prioridade" name="priority" defaultValue={workflow?.priority ?? ""} fullWidth>
        {PRIORITIES.map((priority) => (
          <MenuItem key={priority} value={priority}>
            {PRIORITY_LABELS[priority]}
          </MenuItem>
        ))}
      </TextField>
      <TextField select label="Categoria" name="category" defaultValue={workflow?.category ?? ""} fullWidth>
        <MenuItem value="">Sem categoria</MenuItem>
        {CATEGORIES.map((category) => (
          <MenuItem key={category} value={category}>
            {CATEGORY_LABELS[category]}
          </MenuItem>
        ))}
      </TextField>
    </>
  );
}
