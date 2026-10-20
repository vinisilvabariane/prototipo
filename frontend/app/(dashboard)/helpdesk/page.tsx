import type { Metadata } from "next";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import EmptyState from "@/components/ui/EmptyState";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import SectorCard from "@/features/helpdesk/components/SectorCard";
import { helpdeskService } from "@/features/helpdesk/services/HelpdeskService";

export const metadata: Metadata = {
  title: "Help desk",
  description: "Abra um chamado para o setor certo: o formulário muda conforme o tipo de problema.",
};

export default async function HelpdeskPage() {
  const result = await helpdeskService.sectors();
  if (!result.ok) return <PageError title="Help desk" error={result.error} retryHref="/helpdesk" />;

  const { items, canManage } = result.value;

  return (
    <Stack spacing={2.5}>
      <PageHeader title="Abrir chamado" description="Escolha o setor que resolve o seu problema. O formulário pergunta só o que o setor precisa saber." />

      {items.length === 0 ? (
        <Card>
          <EmptyState
            title="Nenhum setor recebe chamados ainda"
            hint={canManage ? "Crie o primeiro setor e monte o formulário dele." : "Peça a quem gerencia o help desk para criar os setores."}
            action={canManage ? <Button variant="contained" href="/helpdesk/sectors/new">Criar setor</Button> : undefined}
          />
        </Card>
      ) : (
        <Box sx={{ display: "grid", gap: 2, gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "repeat(2, minmax(0, 1fr))", lg: "repeat(3, minmax(0, 1fr))" } }}>
          {items.map((sector) => (
            <SectorCard key={sector.id} sector={sector} />
          ))}
        </Box>
      )}
    </Stack>
  );
}
