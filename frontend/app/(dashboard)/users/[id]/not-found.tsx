import NotFoundState from "@/components/ui/NotFoundState";

export default function UserNotFound() {
  return (
    <NotFoundState
      title="Usuário não encontrado"
      message="Esse usuário não existe ou foi desativado."
      actions={[
        { href: "/users", label: "Ver usuários" },
        { href: "/hub", label: "Ir para o hub" },
      ]}
    />
  );
}
