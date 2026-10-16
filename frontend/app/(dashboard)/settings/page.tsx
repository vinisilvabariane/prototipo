import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import SettingsForm from "@/features/settings/components/SettingsForm";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Configurações",
  description: "Leitura e acessibilidade do MeOne.",
};

export default function SettingsPage() {
  return (
    <Stack spacing={4} sx={{ maxWidth: 760 }}>
      <Stack spacing={0.75}>
        <Typography variant="h4" component="h1">
          Configurações
        </Typography>
        <Typography sx={{ color: inkSoft }}>Leitura e acessibilidade.</Typography>
      </Stack>
      <SettingsForm />
    </Stack>
  );
}
