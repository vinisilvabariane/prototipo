import NotFoundState from "@/components/ui/NotFoundState";

export default function WorkflowNotFound() {
  return (
    <NotFoundState
      title="Workflow não encontrado"
      message="Esse workflow não existe ou o endereço está incorreto."
      actions={[
        { href: "/workflows", label: "Ver workflows" },
        { href: "/hub", label: "Ir para o hub" },
      ]}
    />
  );
}
