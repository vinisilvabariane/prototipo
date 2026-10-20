import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import DataTable from "@/components/ui/DataTable";
import FilterBar from "@/components/ui/FilterBar";
import FormDialog from "@/components/ui/FormDialog";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { departmentService } from "@/features/departments/services/DepartmentService";
import { createUser, deleteUser, updateUser } from "@/features/users/actions/userActions";
import UserFields from "@/features/users/components/UserFields";
import { userService } from "@/features/users/services/UserService";
import { all } from "@/lib/result";

export const metadata: Metadata = {
  title: "Usuários",
  description: "Cadastro e consulta de usuários do sistema.",
};

export default async function UsersPage({ searchParams }: PageProps<"/users">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);
  const search = typeof params.search === "string" ? params.search : "";

  const [users, departments] = await Promise.all([userService.list({ page, search }), departmentService.all()]);

  const data = all({ users, departments });
  if (!data.ok) return <PageError title="Usuários" error={data.error} retryHref="/users" />;
  const { users: list, departments: departmentOptions } = data.value;

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Usuários"
        description="Quem acessa o sistema e em qual departamento trabalha."
        actions={
          <FormDialog title="Novo usuário" trigger="Novo usuário" action={createUser}>
            <UserFields departments={departmentOptions} withPassword />
          </FormDialog>
        }
      />

      <ListCard
        toolbar={<FilterBar basePath="/users" values={{ search }} fields={[{ type: "search", name: "search", label: "Buscar por nome ou matrícula" }]} />}
        footer={<Pager basePath="/users" page={list.page} pageSize={list.pageSize} totalCount={list.totalCount} params={{ search }} />}
      >
        <DataTable
          rows={list.items}
          rowKey={(user) => user.id}
          emptyTitle={search ? "Ninguém encontrado" : "Nenhum usuário cadastrado"}
          emptyHint={search ? "Tente outro nome ou matrícula." : "Cadastre o primeiro usuário em Novo usuário."}
          columns={[
            { key: "registration", header: "Matrícula", fit: true, numeric: true, align: "left", render: (user) => user.registration },
            { key: "name", header: "Nome", render: (user) => <TextLink href={`/users/${user.id}`}>{user.name}</TextLink> },
            { key: "email", header: "E-mail", fit: true, hideBelow: "md", render: (user) => user.email ?? "—" },
            { key: "department", header: "Departamento", fit: true, render: (user) => user.department },
          ]}
          actions={(user) => (
            <>
              <FormDialog iconOnly title="Editar usuário" trigger="Editar" action={updateUser.bind(null, user.id)}>
                <UserFields departments={departmentOptions} user={user} />
              </FormDialog>
              <ConfirmDeleteButton iconOnly name={user.name} action={deleteUser.bind(null, user.id)} />
            </>
          )}
        />
      </ListCard>
    </Stack>
  );
}
