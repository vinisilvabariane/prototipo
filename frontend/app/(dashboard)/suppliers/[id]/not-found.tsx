import NotFoundState from "@/components/ui/NotFoundState";

export default function SupplierNotFound() {
  return (
    <NotFoundState
      title="Fornecedor não encontrado"
      message="Esse fornecedor não existe ou foi removido."
      actions={[
        { href: "/suppliers", label: "Ver fornecedores" },
        { href: "/hub", label: "Ir para o hub" },
      ]}
    />
  );
}
