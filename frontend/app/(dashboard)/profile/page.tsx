import type { Metadata } from "next";
import { redirect } from "next/navigation";
import Avatar from "@mui/material/Avatar";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import { userService } from "@/features/users/services/UserService";
import { readSessionUser } from "@/lib/http/session";
import { all } from "@/lib/result";
import { brandDeep, brandSoft, inkSoft, line } from "@/lib/tokens";
import Field from "@/components/ui/Field";

export const metadata: Metadata = {
  title: "Meu perfil",
  description: "Seus dados de acesso ao MeOne.",
};

const initialsOf = (name: string) =>
  name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

export default async function ProfilePage() {
  const session = await readSessionUser();
  if (!session || session.id === null) redirect("/login");

  const [user, roles] = await Promise.all([userService.getById(session.id), userService.roles(session.id)]);
  const data = all({ user, roles });
  if (!data.ok) return <PageError title="Meu perfil" error={data.error} retryHref="/profile" />;
  const { user: person, roles: assigned } = data.value;

  return (
    <Stack spacing={4} sx={{ maxWidth: 760 }}>
      <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
        <Avatar sx={{ width: 56, height: 56, fontSize: "1.2rem", fontWeight: 700, bgcolor: brandSoft, color: brandDeep }}>
          {initialsOf(person.name)}
        </Avatar>
        <Box>
          <Typography variant="h4" component="h1">
            {person.name}
          </Typography>
          <Typography sx={{ color: inkSoft, mt: 0.5 }}>
            Matrícula{" "}
            <Box component="span" sx={{ fontVariantNumeric: "tabular-nums", color: "text.primary" }}>
              {person.registration}
            </Box>
          </Typography>
        </Box>
      </Box>

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" }, alignItems: "start" }}>
        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Dados
          </Typography>
          <Box sx={{ mt: 1 }}>
            <Field label="E-mail">{person.email ?? "—"}</Field>
            <Field label="Departamento">{person.department}</Field>
          </Box>
        </Card>

        <Card sx={{ p: 3 }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Perfis ({assigned.length})
          </Typography>
          {assigned.length === 0 ? (
            <Typography sx={{ mt: 2, color: inkSoft }}>Você ainda não tem nenhum perfil atribuído.</Typography>
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

      <Typography variant="body2" sx={{ color: inkSoft }}>
        Para alterar seus dados ou sua senha, fale com um administrador do sistema.
      </Typography>
    </Stack>
  );
}
