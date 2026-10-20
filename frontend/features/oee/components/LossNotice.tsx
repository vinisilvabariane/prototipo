import Alert from "@mui/material/Alert";

export default function LossNotice({ available }: { available: boolean }) {
  if (available) return null;

  return (
    <Alert severity="info" variant="outlined">
      Paradas e defeitos ainda não entram no cálculo: eles virão do módulo de help desk. Por enquanto a disponibilidade e a qualidade ficam em 100% e o desempenho mede
      o produzido contra a meta por hora.
    </Alert>
  );
}
