import NotFoundState from "@/components/ui/NotFoundState";

export default function LayoutNotFound() {
  return (
    <NotFoundState
      title="Layout não encontrado"
      message="Esse layout não existe ou foi excluído."
      actions={[
        { href: "/production/layouts", label: "Ver layouts" },
        { href: "/hub", label: "Ir para o hub" },
      ]}
    />
  );
}
