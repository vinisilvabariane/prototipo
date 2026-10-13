import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import type { Department } from "@/features/departments/types";
import type { User } from "../types";

type Props = {
  departments: Department[];
  user?: User;
  withPassword?: boolean;
};

export default function UserFields({ departments, user, withPassword }: Props) {
  return (
    <>
      <TextField label="Matrícula" name="registration" defaultValue={user?.registration} slotProps={{ htmlInput: { inputMode: "numeric" } }} fullWidth />
      <TextField label="Nome" name="name" defaultValue={user?.name} fullWidth />
      <TextField label="E-mail" name="email" type="email" defaultValue={user?.email ?? ""} fullWidth />
      <TextField select label="Departamento" name="department" defaultValue={user?.department ?? ""} fullWidth>
        {departments.map((d) => (
          <MenuItem key={d.id} value={d.name}>
            {d.name}
          </MenuItem>
        ))}
      </TextField>
      {withPassword && <TextField label="Senha" name="password" type="password" fullWidth />}
    </>
  );
}
