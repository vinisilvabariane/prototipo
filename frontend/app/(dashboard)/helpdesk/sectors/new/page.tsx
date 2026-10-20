import type { Metadata } from "next";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import SectorBuilder from "@/features/helpdesk/builder/SectorBuilder";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";
import { all } from "@/lib/result";

export const metadata: Metadata = { title: "Help desk · Novo setor" };

export default async function NewSectorPage() {
  const [sectors, templates, roles, locations] = await Promise.all([helpdeskService.sectors(), helpdeskService.templates(), helpdeskService.roles(), helpdeskService.locations()]);

  if (!sectors.ok) return <PageError title="Novo setor" error={sectors.error} retryHref="/helpdesk/sectors/new" />;

  if (!sectors.value.canManage) {
    return (
      <Card>
        <EmptyState title="Só quem gerencia o help desk monta setores" />
      </Card>
    );
  }

  const loaded = all({ templates, roles, locations });
  if (!loaded.ok) return <PageError title="Novo setor" error={loaded.error} retryHref="/helpdesk/sectors/new" />;

  return (
    <Stack spacing={2.5}>
      <PageHeader back={{ href: "/helpdesk/sectors", label: "Setores" }} title="Novo setor" description="Defina a identidade, monte o formulário e escolha quem atende." />
      <SectorBuilder sector={null} templates={loaded.value.templates} roles={loaded.value.roles} locations={loaded.value.locations} />
    </Stack>
  );
}
