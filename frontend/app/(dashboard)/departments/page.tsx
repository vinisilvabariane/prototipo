import type { Metadata } from "next";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
import { createDepartment, deleteDepartment, updateDepartment } from "@/features/departments/actions/departmentActions";
import DepartmentFields from "@/features/departments/components/DepartmentFields";
import { departmentService } from "@/features/departments/services/DepartmentService";

export const metadata: Metadata = {
  title: "Departamentos",
  description: "Departamentos cadastrados no sistema.",
};

export default async function DepartmentsPage() {
  const result = await departmentService.all();
  if (!result.ok) return <PageError title="Departamentos" error={result.error} retryHref="/departments" />;
  const departments = result.value;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4">Departamentos</Typography>
        <FormDialog title="Novo departamento" trigger="Novo departamento" action={createDepartment}>
          <DepartmentFields />
        </FormDialog>
      </Stack>

      <Card variant="outlined">
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {departments.map((d) => (
              <TableRow key={d.id}>
                <TableCell>{d.name}</TableCell>
                <TableCell align="right">
                  <FormDialog compact title="Editar departamento" trigger="Editar" action={updateDepartment.bind(null, d.id)}>
                    <DepartmentFields department={d} />
                  </FormDialog>
                  <ConfirmDeleteButton name={d.name} action={deleteDepartment.bind(null, d.id)} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </Stack>
  );
}
