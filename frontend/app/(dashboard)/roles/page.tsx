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
import Pager from "@/components/ui/Pager";
import { createRole, deleteRole, updateRole } from "@/features/roles/actions/roleActions";
import RoleFields from "@/features/roles/components/RoleFields";
import { roleService } from "@/features/roles/services/RoleService";

export default async function RolesPage({ searchParams }: PageProps<"/roles">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);

  const result = await roleService.list(page);
  if (!result.ok) return <PageError title="Perfis" error={result.error} retryHref="/roles" />;
  const roles = result.value;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4">Perfis</Typography>
        <FormDialog title="Novo perfil" trigger="Novo perfil" action={createRole}>
          <RoleFields />
        </FormDialog>
      </Stack>

      <Card variant="outlined">
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nome</TableCell>
              <TableCell>Descrição</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {roles.items.map((r) => (
              <TableRow key={r.id}>
                <TableCell>{r.name}</TableCell>
                <TableCell>{r.description}</TableCell>
                <TableCell align="right">
                  <FormDialog compact title="Editar perfil" trigger="Editar" action={updateRole.bind(null, r.id)}>
                    <RoleFields role={r} />
                  </FormDialog>
                  <ConfirmDeleteButton name={r.name} action={deleteRole.bind(null, r.id)} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <Pager basePath="/roles" page={roles.page} pageSize={roles.pageSize} totalCount={roles.totalCount} />
      </Card>
    </Stack>
  );
}
