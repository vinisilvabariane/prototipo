import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import DataTable from "@/components/ui/DataTable";
import FormDialog from "@/components/ui/FormDialog";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Pager from "@/components/ui/Pager";
import { createRole, deleteRole, updateRole } from "@/features/roles/actions/roleActions";
import RoleFields from "@/features/roles/components/RoleFields";
import { roleService } from "@/features/roles/services/RoleService";

export const metadata: Metadata = {
  title: "Perfis",
  description: "Perfis de acesso do sistema.",
};

export default async function RolesPage({ searchParams }: PageProps<"/roles">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);

  const result = await roleService.list(page);
  if (!result.ok) return <PageError title="Perfis" error={result.error} retryHref="/roles" />;
  const roles = result.value;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Perfis"
        description="Conjuntos de permissões que você atribui às pessoas."
        actions={
          <FormDialog title="Novo perfil" trigger="Novo perfil" action={createRole}>
            <RoleFields />
          </FormDialog>
        }
      />

      <ListCard footer={<Pager basePath="/roles" page={roles.page} pageSize={roles.pageSize} totalCount={roles.totalCount} />}>
        <DataTable
          rows={roles.items}
          rowKey={(role) => role.id}
          emptyTitle="Nenhum perfil cadastrado"
          emptyHint="Crie o primeiro perfil em Novo perfil."
          columns={[
            { key: "name", header: "Nome", strong: true, fit: true, render: (role) => role.name },
            { key: "description", header: "Descrição", render: (role) => role.description },
          ]}
          actions={(role) => (
            <>
              <FormDialog iconOnly title="Editar perfil" trigger="Editar" action={updateRole.bind(null, role.id)}>
                <RoleFields role={role} />
              </FormDialog>
              <ConfirmDeleteButton iconOnly name={role.name} action={deleteRole.bind(null, role.id)} />
            </>
          )}
        />
      </ListCard>
    </Stack>
  );
}
