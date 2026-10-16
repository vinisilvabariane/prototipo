import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import { departmentService } from "@/features/departments/services/DepartmentService";
import OrgChartView from "@/features/org-chart/components/OrgChartView";
import { orgChartService } from "@/features/org-chart/services/OrgChartService";
import { all, ok } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Organograma",
  description: "Estrutura hierárquica de cargos e quem ocupa cada um.",
};

export default async function OrgChartPage() {
  const chart = await orgChartService.chart();
  if (!chart.ok) return <PageError title="Organograma" error={chart.error} retryHref="/org-chart" />;

  const [departments, users] = await Promise.all([
    departmentService.all(),
    chart.value.canManage ? orgChartService.assignableUsers() : Promise.resolve(ok([])),
  ]);

  const data = all({ departments, users });
  if (!data.ok) return <PageError title="Organograma" error={data.error} retryHref="/org-chart" />;

  return (
    <Stack spacing={3}>
      <div>
        <Typography variant="h4" component="h1">
          Organograma
        </Typography>
        <Typography sx={{ mt: 0.75, color: inkSoft }}>Quem responde a quem, e quais cargos estão vagos.</Typography>
      </div>
      <OrgChartView chart={chart.value} departments={data.value.departments} users={data.value.users} />
    </Stack>
  );
}
