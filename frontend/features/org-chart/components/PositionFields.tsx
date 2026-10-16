import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import type { Department } from "@/features/departments/types";
import type { OrgPosition, OrgUser } from "../types";

type Props = {
  departments: Department[];
  position?: OrgPosition;
  defaultDepartmentId?: number;
  holders?: OrgUser[];
};

export default function PositionFields({ departments, position, defaultDepartmentId, holders }: Props) {
  return (
    <>
      <TextField label="Nome do cargo" name="title" defaultValue={position?.title} slotProps={{ htmlInput: { maxLength: 100 } }} autoFocus fullWidth />
      <TextField select label="Departamento" name="departmentId" defaultValue={position?.departmentId ?? defaultDepartmentId ?? ""} fullWidth>
        {departments.map((department) => (
          <MenuItem key={department.id} value={department.id}>
            {department.name}
          </MenuItem>
        ))}
      </TextField>
      {holders && (
        <TextField select label="Ocupante" name="holderId" defaultValue="" helperText="Deixe em branco para criar o cargo vago." fullWidth>
          <MenuItem value="">Vago</MenuItem>
          {holders.map((user) => (
            <MenuItem key={user.id} value={user.id}>
              {user.name} ({user.registration})
            </MenuItem>
          ))}
        </TextField>
      )}
    </>
  );
}
