import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import OpenTicketForm from "@/features/helpdesk/components/OpenTicketForm";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";
import { all } from "@/lib/result";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/helpdesk/new/[sectorId]">): Promise<Metadata> {
  const id = parseId((await params).sectorId);
  if (id === null) return { title: "Setor não encontrado" };

  const result = await helpdeskService.sector(id);
  return { title: result.ok ? `Abrir chamado · ${result.value.name}` : "Abrir chamado" };
}

export default async function NewTicketPage({ params }: PageProps<"/helpdesk/new/[sectorId]">) {
  const id = parseId((await params).sectorId);
  if (id === null) notFound();

  const [sector, locations] = await Promise.all([helpdeskService.sector(id), helpdeskService.locations()]);
  if (!sector.ok && sector.error.code === "notFound") notFound();

  const loaded = all({ sector, locations });
  if (!loaded.ok) return <PageError title="Abrir chamado" error={loaded.error} retryHref={`/helpdesk/new/${id}`} />;

  const data = loaded.value.sector;
  if (data.externalUrl) redirect(data.externalUrl);

  return (
    <Stack spacing={2.5}>
      <PageHeader back={{ href: "/helpdesk", label: "Setores" }} eyebrow="Novo chamado" title={data.name} description={data.description ?? undefined} />
      {!data.active ? (
        <PageError title="Setor desativado" error={{ code: "badRequest", status: null, title: "Setor desativado", messages: ["Este setor não recebe chamados no momento."] }} retryHref="/helpdesk" />
      ) : (
        <OpenTicketForm sector={data} locations={loaded.value.locations} />
      )}
    </Stack>
  );
}
