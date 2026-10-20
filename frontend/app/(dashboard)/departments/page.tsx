import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import DataTable from "@/components/ui/DataTable";
import FormDialog from "@/components/ui/FormDialog";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
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
    <Stack spacing={2.5}>
      <PageHeader
        title="Departamentos"
        description="Áreas da empresa a que cada pessoa pertence."
        actions={
          <FormDialog title="Novo departamento" trigger="Novo departamento" action={createDepartment}>
            <DepartmentFields />
          </FormDialog>
        }
      />

      <ListCard>
        <DataTable
          rows={departments}
          rowKey={(department) => department.id}
          emptyTitle="Nenhum departamento cadastrado"
          emptyHint="Crie o primeiro em Novo departamento."
          columns={[{ key: "name", header: "Nome", strong: true, render: (department) => department.name }]}
          actions={(department) => (
            <>
              <FormDialog iconOnly title="Editar departamento" trigger="Editar" action={updateDepartment.bind(null, department.id)}>
                <DepartmentFields department={department} />
              </FormDialog>
              <ConfirmDeleteButton iconOnly name={department.name} action={deleteDepartment.bind(null, department.id)} />
            </>
          )}
        />
      </ListCard>
    </Stack>
  );
}
