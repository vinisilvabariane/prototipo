import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import SectorBuilder from "@/features/helpdesk/builder/SectorBuilder";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";
import { all } from "@/lib/result";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/helpdesk/sectors/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Setor não encontrado" };

  const result = await helpdeskService.sector(id);
  return { title: result.ok ? `Setor · ${result.value.name}` : "Setor" };
}

export default async function EditSectorPage({ params }: PageProps<"/helpdesk/sectors/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const [sector, templates, roles, locations] = await Promise.all([helpdeskService.sector(id), helpdeskService.templates(), helpdeskService.roles(), helpdeskService.locations()]);
  if (!sector.ok && sector.error.code === "notFound") notFound();
  if (!sector.ok) return <PageError title="Setor" error={sector.error} retryHref={`/helpdesk/sectors/${id}`} />;

  if (!sector.value.canManage) {
    return (
      <Card>
        <EmptyState title="Só quem gerencia o help desk edita setores" />
      </Card>
    );
  }

  const loaded = all({ templates, roles, locations });
  if (!loaded.ok) return <PageError title="Setor" error={loaded.error} retryHref={`/helpdesk/sectors/${id}`} />;

  return (
    <Stack spacing={2.5}>
      <PageHeader back={{ href: "/helpdesk/sectors", label: "Setores" }} eyebrow="Editar setor" title={sector.value.name} />
      <SectorBuilder key={`${sector.value.id}`} sector={sector.value} templates={loaded.value.templates} roles={loaded.value.roles} locations={loaded.value.locations} />
    </Stack>
  );
}
