import NotFoundState from "@/components/ui/NotFoundState";

export default function InstructionNotFound() {
  return (
    <NotFoundState
      title="Instrução não encontrada"
      message="Essa instrução de trabalho não existe ou foi excluída."
      actions={[
        { href: "/instructions", label: "Ver instruções" },
        { href: "/hub", label: "Ir para o hub" },
      ]}
    />
  );
}
