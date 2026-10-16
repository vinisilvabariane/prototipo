import type { Metadata } from "next";
import { notFound } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import { userService } from "@/features/users/services/UserService";
import { all } from "@/lib/result";
import { inkSoft, line } from "@/lib/tokens";
import Field from "@/components/ui/Field";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/users/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Usuário não encontrado" };

  const result = await userService.getById(id);
  if (result.ok) return { title: result.value.name };
  return { title: result.error.code === "notFound" ? "Usuário não encontrado" : "Usuário" };
}

export default async function UserPage({ params }: PageProps<"/users/[id]">) {
  const userId = parseId((await params).id);
  if (userId === null) notFound();

  const [user, roles] = await Promise.all([userService.getById(userId), userService.roles(userId)]);
  if (!user.ok && user.error.code === "notFound") notFound();

  const data = all({ user, roles });
  if (!data.ok) return <PageError title="Usuário" error={data.error} retryHref={`/users/${userId}`} />;
  const { user: person, roles: assigned } = data.value;

  return (
    <Stack spacing={4}>
      <Box>
        <Button href="/users" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Usuários
        </Button>
        <Typography variant="h4" component="h1" sx={{ mt: 1 }}>
          {person.name}
        </Typography>
      </Box>

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" }, alignItems: "start" }}>
        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Dados
          </Typography>
          <Box sx={{ mt: 1 }}>
            <Field label="Matrícula">
              <Box component="span" sx={{ fontVariantNumeric: "tabular-nums" }}>
                {person.registration}
              </Box>
            </Field>
            <Field label="E-mail">{person.email ?? "—"}</Field>
            <Field label="Departamento">{person.department}</Field>
          </Box>
        </Card>

        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Perfis ({assigned.length})
          </Typography>
          {assigned.length === 0 ? (
            <Typography sx={{ mt: 2, color: inkSoft }}>Nenhum perfil atribuído a este usuário.</Typography>
          ) : (
            <Box component="ul" sx={{ listStyle: "none", m: 0, mt: 1, p: 0 }}>
              {assigned.map((role) => (
                <Box key={role.id} component="li" sx={{ py: 1.5, borderBottom: `1px solid ${line}`, "&:last-of-type": { borderBottom: 0 } }}>
                  <Typography sx={{ fontWeight: 600 }}>{role.name}</Typography>
                  <Typography variant="body2" sx={{ color: inkSoft }}>
                    {role.description}
                  </Typography>
                </Box>
              ))}
            </Box>
          )}
        </Card>
      </Box>
    </Stack>
  );
}
