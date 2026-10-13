import TextField from "@mui/material/TextField";
import type { Department } from "@/features/departments/types";

export default function DepartmentFields({ department }: { department?: Department }) {
  return <TextField label="Nome" name="name" defaultValue={department?.name} fullWidth />;
}
