import type { Metadata } from "next";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import DataTable from "@/components/ui/DataTable";
import FilterBar from "@/components/ui/FilterBar";
import ListCard from "@/components/ui/ListCard";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { CriticalityChip, QualificationChip } from "@/features/suppliers/components/SupplierBadges";
import { formatDay, formatScore } from "@/features/suppliers/format";
import {
  CRITICALITIES,
  CRITICALITY_LABELS,
  MULTI_BRANCHES,
  MULTI_BRANCH_LABELS,
  ORIGINS,
  ORIGIN_LABELS,
  QUALIFICATION_KIND_LABELS,
  STATUS_FILTERS,
  STATUS_FILTER_LABELS,
} from "@/features/suppliers/labels";
import { supplierService } from "@/features/suppliers/services/SupplierService";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = {
  title: "Fornecedores",
  description: "Cadastro e qualificação de fornecedores.",
};

function pick<T extends string>(value: string | string[] | undefined, allowed: readonly T[]): T | undefined {
  return typeof value === "string" ? allowed.find((item) => item === value) : undefined;
}

export default async function SuppliersPage({ searchParams }: PageProps<"/suppliers">) {
  const params = await searchParams;
  const page = Math.max(0, Number(params.page) || 0);
  const search = typeof params.search === "string" ? params.search : "";
  const origin = pick(params.origin, ORIGINS);
  const multiBranch = pick(params.multiBranch, MULTI_BRANCHES);
  const criticality = pick(params.criticality, CRITICALITIES);
  const status = pick(params.status, STATUS_FILTERS);

  const result = await supplierService.list({ page, search, origin, multiBranch, criticality, status });
  if (!result.ok) return <PageError title="Fornecedores" error={result.error} retryHref="/suppliers" />;
  const list = result.value;

  const filtered = Boolean(search || origin || multiBranch || criticality || status);

  return (
    <Stack spacing={2.5}>
      <PageHeader
        title="Fornecedores"
        description="Cadastro, qualificação e documentos de cada fornecedor."
        actions={
          list.canRegister && (
            <Button href="/suppliers/new" variant="contained">
              Novo fornecedor
            </Button>
          )
        }
      />

      <ListCard
        toolbar={
          <FilterBar
            basePath="/suppliers"
            values={{ search, status: status ?? "", multiBranch: multiBranch ?? "", criticality: criticality ?? "", origin: origin ?? "" }}
            fields={[
              { type: "search", name: "search", label: "Buscar por nome, código ou CNPJ", width: 300 },
              { type: "select", name: "status", label: "Status", options: STATUS_FILTERS.map((item) => ({ value: item, label: STATUS_FILTER_LABELS[item] })) },
              { type: "select", name: "multiBranch", label: "Filial Multi", options: MULTI_BRANCHES.map((item) => ({ value: item, label: MULTI_BRANCH_LABELS[item] })) },
              { type: "select", name: "criticality", label: "Criticidade", width: 150, options: CRITICALITIES.map((item) => ({ value: item, label: CRITICALITY_LABELS[item] })) },
              { type: "select", name: "origin", label: "Origem", width: 150, options: ORIGINS.map((item) => ({ value: item, label: ORIGIN_LABELS[item] })) },
            ]}
          />
        }
        footer={<Pager basePath="/suppliers" page={list.page} pageSize={list.pageSize} totalCount={list.totalCount} params={{ search, origin, multiBranch, criticality, status }} />}
      >
        <DataTable
          rows={list.items}
          rowKey={(item) => item.id}
          emptyTitle={filtered ? "Nenhum fornecedor encontrado" : "Nenhum fornecedor cadastrado"}
          emptyHint={filtered ? "Ajuste ou limpe os filtros." : list.canRegister ? "Cadastre o primeiro em Novo fornecedor." : undefined}
          columns={[
            { key: "sapCode", header: "Código SAP", fit: true, numeric: true, align: "left", render: (item) => item.sapCode },
            {
              key: "name",
              header: "Fornecedor",
              wrap: true,
              render: (item) => (
                <>
                  <TextLink href={`/suppliers/${item.id}`}>{item.name}</TextLink>
                  <Typography component="span" variant="body2" sx={{ color: inkSoft, ml: 1 }}>
                    {item.branch}
                  </Typography>
                </>
              ),
            },
            { key: "multiBranch", header: "Filial Multi", fit: true, hideBelow: "md", render: (item) => MULTI_BRANCH_LABELS[item.multiBranch] },
            { key: "origin", header: "Origem", fit: true, hideBelow: "lg", render: (item) => ORIGIN_LABELS[item.origin] },
            { key: "criticality", header: "Criticidade", fit: true, render: (item) => <CriticalityChip criticality={item.criticality} /> },
            {
              key: "qualification",
              header: "Qualificação",
              fit: true,
              render: (item) => (
                <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                  <QualificationChip qualification={item.qualification} />
                  {item.qualification && (
                    <Typography variant="body2" sx={{ color: inkSoft, display: { xs: "none", lg: "block" } }}>
                      {QUALIFICATION_KIND_LABELS[item.qualification.kind]} · {formatScore(item.qualification.score)}
                    </Typography>
                  )}
                </Stack>
              ),
            },
            { key: "expiresOn", header: "Vencimento", fit: true, numeric: true, align: "left", render: (item) => formatDay(item.qualification?.expiresOn) },
          ]}
        />
      </ListCard>
    </Stack>
  );
}
