import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import { departmentService } from "@/features/departments/services/DepartmentService";
import OrgChartView from "@/features/org-chart/components/OrgChartView";
import { orgChartService } from "@/features/org-chart/services/OrgChartService";
import { all, ok } from "@/lib/result";

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
    <Stack spacing={2.5}>
      <PageHeader title="Organograma" description="Quem responde a quem, e quais cargos estão vagos." />
      <OrgChartView chart={chart.value} departments={data.value.departments} users={data.value.users} />
    </Stack>
  );
}
