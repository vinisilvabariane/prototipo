import type { Metadata } from "next";
import { redirect } from "next/navigation";
import Avatar from "@mui/material/Avatar";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import Panel from "@/components/ui/Panel";
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
    <Stack spacing={2.5} sx={{ maxWidth: 820 }}>
      <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
        <Avatar sx={{ width: 56, height: 56, fontSize: "1.2rem", fontWeight: 700, bgcolor: brandSoft, color: brandDeep }}>
          {initialsOf(person.name)}
        </Avatar>
        <Box>
          <Typography variant="h4" component="h1">
            {person.name}
          </Typography>
          <Typography variant="body2" sx={{ color: inkSoft, mt: 0.25 }}>
            Matrícula{" "}
            <Box component="span" sx={{ fontVariantNumeric: "tabular-nums", color: "text.primary", fontWeight: 600 }}>
              {person.registration}
            </Box>
          </Typography>
        </Box>
      </Box>

      <Box sx={{ display: "grid", gap: 2.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" }, alignItems: "start" }}>
        <Panel title="Dados">
          <Field label="E-mail">{person.email ?? "—"}</Field>
          <Field label="Departamento">{person.department}</Field>
        </Panel>

        <Panel title="Perfis" count={assigned.length}>
          {assigned.length === 0 ? (
            <EmptyState compact title="Nenhum perfil atribuído" hint="Peça a um administrador para atribuir um perfil a você." />
          ) : (
            <Box component="ul" sx={{ listStyle: "none", m: 0, p: 0 }}>
              {assigned.map((role) => (
                <Box key={role.id} component="li" sx={{ py: 1, borderBottom: `1px solid ${line}`, "&:last-of-type": { borderBottom: 0 } }}>
                  <Typography sx={{ fontWeight: 600 }}>{role.name}</Typography>
                  <Typography variant="body2" sx={{ color: inkSoft }}>
                    {role.description}
                  </Typography>
                </Box>
              ))}
            </Box>
          )}
        </Panel>
      </Box>

      <Typography variant="body2" sx={{ color: inkSoft }}>
        Para alterar seus dados ou sua senha, fale com um administrador do sistema.
      </Typography>
    </Stack>
  );
}
