import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import { formatDateTime } from "@/features/oee/format";
import { oeeService } from "@/features/oee/services/OeeService";
import FormulaStudio from "@/features/oee/studio/FormulaStudio";
import { all } from "@/lib/result";

export const metadata: Metadata = {
  title: "OEE · Fórmulas",
  description: "Monte e teste as fórmulas de disponibilidade, desempenho, qualidade e OEE de posto e linha.",
};

export default async function OeeFormulasPage() {
  const [config, runs] = await Promise.all([oeeService.config(), oeeService.runs({ from: "", to: "" })]);
  const loaded = all({ config, runs });
  if (!loaded.ok) return <PageError title="Fórmulas de OEE" error={loaded.error} retryHref="/oee/formulas" />;

  const testRuns = loaded.value.runs.runs.map((run) => ({
    id: run.runId,
    label: `${run.lineName} · ${run.productCode} · ${formatDateTime(run.startedAt)}`,
  }));

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Fórmulas de OEE"
        description="Cada pilar e o OEE de posto e de linha são uma expressão. Combine variáveis, operadores e funções, teste com números de uma produção real e use o conjunto que fizer sentido para a fábrica."
      />
      <FormulaStudio config={loaded.value.config} testRuns={testRuns} />
    </Stack>
  );
}
