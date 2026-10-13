import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import FormDialog from "@/components/ui/FormDialog";
import PageError from "@/components/ui/PageError";
import Pager from "@/components/ui/Pager";
import { departmentService } from "@/features/departments/services/DepartmentService";
import { createUser, deleteUser, updateUser } from "@/features/users/actions/userActions";
import UserFields from "@/features/users/components/UserFields";
import { userService } from "@/features/users/services/UserService";
import { all } from "@/lib/result";
import { monoFont } from "@/lib/tokens";

export default async function UsersPage({ searchParams }: PageProps<"/users">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);
  const search = typeof params.search === "string" ? params.search : "";

  const [users, departments] = await Promise.all([userService.list({ page, search }), departmentService.all()]);

  const data = all({ users, departments });
  if (!data.ok) return <PageError title="Usuários" error={data.error} retryHref="/users" />;
  const { users: list, departments: departmentOptions } = data.value;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4">Usuários</Typography>
        <FormDialog title="Novo usuário" trigger="Novo usuário" action={createUser}>
          <UserFields departments={departmentOptions} withPassword />
        </FormDialog>
      </Stack>

      <Box component="form" sx={{ display: "flex", gap: 1 }}>
        <TextField name="search" label="Buscar" size="small" defaultValue={search} />
        <Button type="submit" variant="outlined">
          Buscar
        </Button>
      </Box>

      <Card variant="outlined">
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Matrícula</TableCell>
              <TableCell>Nome</TableCell>
              <TableCell>E-mail</TableCell>
              <TableCell>Departamento</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {list.items.map((u) => (
              <TableRow key={u.id}>
                <TableCell sx={{ fontFamily: monoFont }}>{u.registration}</TableCell>
                <TableCell>{u.name}</TableCell>
                <TableCell>{u.email}</TableCell>
                <TableCell>{u.department}</TableCell>
                <TableCell align="right">
                  <FormDialog compact title="Editar usuário" trigger="Editar" action={updateUser.bind(null, u.id)}>
                    <UserFields departments={departmentOptions} user={u} />
                  </FormDialog>
                  <ConfirmDeleteButton name={u.name} action={deleteUser.bind(null, u.id)} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <Pager basePath="/users" page={list.page} pageSize={list.pageSize} totalCount={list.totalCount} params={{ search }} />
      </Card>
    </Stack>
  );
}
