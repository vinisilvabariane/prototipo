import TextField from "@mui/material/TextField";
import type { Role } from "@/features/roles/types";

export default function RoleFields({ role }: { role?: Role }) {
  return (
    <>
      <TextField label="Nome" name="name" defaultValue={role?.name} fullWidth />
      <TextField label="Descrição" name="description" defaultValue={role?.description} multiline minRows={2} fullWidth />
    </>
  );
}
