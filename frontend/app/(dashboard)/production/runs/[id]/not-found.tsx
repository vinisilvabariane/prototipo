import NotFoundState from "@/components/ui/NotFoundState";

export default function RunNotFound() {
  return (
    <NotFoundState
      title="Produção não encontrada"
      message="Essa produção não existe ou foi cancelada."
      actions={[
        { href: "/production", label: "Ver produções" },
        { href: "/hub", label: "Ir para o hub" },
      ]}
    />
  );
}
