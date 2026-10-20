import type { Metadata } from "next";
import AddOutlined from "@mui/icons-material/AddOutlined";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import DataTable from "@/components/ui/DataTable";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import TextLink from "@/components/ui/TextLink";
import { HelpdeskIcon } from "@/features/helpdesk/icons";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";

export const metadata: Metadata = {
  title: "Help desk · Setores",
  description: "Setores que recebem chamados, com formulário, resposta do técnico e quem atende.",
};

export default async function SectorsPage() {
  const result = await helpdeskService.sectors(true);
  if (!result.ok) return <PageError title="Setores" error={result.error} retryHref="/helpdesk/sectors" />;

  const { items, canManage } = result.value;

  if (!canManage) {
    return (
      <Stack spacing={2.5}>
        <PageHeader title="Setores" />
        <Card>
          <EmptyState title="Só quem gerencia o help desk monta setores" hint="Peça o perfil “gestor-helpdesk” a um administrador para criar setores e formulários." />
        </Card>
      </Stack>
    );
  }

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Setores"
        description="Cada setor tem o próprio formulário de abertura, formulário de resposta do técnico e perfis que atendem."
        actions={
          <Button variant="contained" startIcon={<AddOutlined />} href="/helpdesk/sectors/new">
            Novo setor
          </Button>
        }
      />

      <Card sx={{ overflow: "hidden" }}>
        <DataTable
          rows={items}
          rowKey={(sector) => sector.id}
          emptyTitle="Nenhum setor ainda"
          emptyHint="Crie o primeiro setor e monte o formulário dele."
          columns={[
            {
              key: "name",
              header: "Setor",
              strong: true,
              render: (sector) => (
                <Stack direction="row" spacing={1.25} sx={{ alignItems: "center" }}>
                  <Box sx={{ width: 28, height: 28, borderRadius: 1, display: "grid", placeItems: "center", color: "#fff", bgcolor: sector.colorHex, flexShrink: 0 }}>
                    <HelpdeskIcon name={sector.icon} sx={{ fontSize: 18 }} />
                  </Box>
                  <TextLink href={`/helpdesk/sectors/${sector.id}`}>{sector.name}</TextLink>
                </Stack>
              ),
            },
            { key: "kind", header: "Tipo", fit: true, hideBelow: "md", render: (sector) => (sector.externalUrl ? "Externo" : `${sector.fieldCount} pergunta(s)`) },
            { key: "badges", header: "Etiquetas", hideBelow: "lg", render: (sector) => (sector.badges.length === 0 ? "—" : sector.badges.map((badge) => <Chip key={badge} size="small" variant="outlined" label={badge} sx={{ mr: 0.5 }} />)) },
            { key: "active", header: "Situação", fit: true, render: (sector) => <Chip size="small" color={sector.active ? "success" : "default"} variant={sector.active ? "filled" : "outlined"} label={sector.active ? "Ativo" : "Desativado"} /> },
          ]}
          actions={(sector) => (
            <Button size="small" href={`/helpdesk/sectors/${sector.id}`}>
              Editar
            </Button>
          )}
        />
      </Card>
    </Stack>
  );
}
