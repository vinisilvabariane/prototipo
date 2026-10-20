import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import PageHeader from "@/components/ui/PageHeader";
import SettingsForm from "@/features/settings/components/SettingsForm";

export const metadata: Metadata = {
  title: "Configurações",
  description: "Leitura e acessibilidade do MeOne.",
};

export default function SettingsPage() {
  return (
    <Stack spacing={2.5} sx={{ maxWidth: 760 }}>
      <PageHeader title="Configurações" description="Leitura e acessibilidade do sistema." />
      <SettingsForm />
    </Stack>
  );
}
