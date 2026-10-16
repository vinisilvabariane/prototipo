import NotFoundState from "@/components/ui/NotFoundState";

export default function NotFound() {
  return (
    <NotFoundState
      fullPage
      title="Página não encontrada"
      message="O endereço não existe ou foi movido. Volte ao hub para escolher um módulo."
      actions={[{ href: "/hub", label: "Ir para o hub" }]}
    />
  );
}
