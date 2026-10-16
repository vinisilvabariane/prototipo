import type { Metadata } from "next";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import Pager from "@/components/ui/Pager";
import TextLink from "@/components/ui/TextLink";
import { CriticalityChip, QualificationChip } from "@/features/suppliers/components/SupplierBadges";
import SupplierFilters from "@/features/suppliers/components/SupplierFilters";
import { formatDay, formatScore } from "@/features/suppliers/format";
import {
  CRITICALITIES,
  MULTI_BRANCHES,
  MULTI_BRANCH_LABELS,
  ORIGINS,
  ORIGIN_LABELS,
  QUALIFICATION_KIND_LABELS,
  STATUS_FILTERS,
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

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Fornecedores
        </Typography>
        {list.canRegister && (
          <Button href="/suppliers/new" variant="contained">
            Novo fornecedor
          </Button>
        )}
      </Stack>

      <SupplierFilters values={{ search, origin: origin ?? "", multiBranch: multiBranch ?? "", criticality: criticality ?? "", status: status ?? "" }} />

      <Card variant="outlined">
        <Box sx={{ overflowX: "auto" }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Código SAP</TableCell>
                <TableCell>Fornecedor</TableCell>
                <TableCell>Filial Multi</TableCell>
                <TableCell>Origem</TableCell>
                <TableCell>Criticidade</TableCell>
                <TableCell>Qualificação</TableCell>
                <TableCell>Vencimento</TableCell>
                <TableCell align="right">Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {list.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8} sx={{ color: inkSoft, textAlign: "center", py: 4 }}>
                    Nenhum fornecedor encontrado.
                  </TableCell>
                </TableRow>
              )}
              {list.items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{item.sapCode}</TableCell>
                  <TableCell sx={{ maxWidth: 360 }}>
                    <TextLink href={`/suppliers/${item.id}`}>{item.name}</TextLink>
                    <Typography component="span" variant="body2" sx={{ color: inkSoft, ml: 1 }}>
                      {item.branch}
                    </Typography>
                  </TableCell>
                  <TableCell>{MULTI_BRANCH_LABELS[item.multiBranch]}</TableCell>
                  <TableCell>{ORIGIN_LABELS[item.origin]}</TableCell>
                  <TableCell>
                    <CriticalityChip criticality={item.criticality} />
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                      <QualificationChip qualification={item.qualification} />
                      {item.qualification && (
                        <Typography variant="body2" sx={{ color: inkSoft, whiteSpace: "nowrap" }}>
                          {QUALIFICATION_KIND_LABELS[item.qualification.kind]} · {formatScore(item.qualification.score)}
                        </Typography>
                      )}
                    </Stack>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums" }}>{formatDay(item.qualification?.expiresOn)}</TableCell>
                  <TableCell align="right">
                    <Button size="small" href={`/suppliers/${item.id}`}>
                      Detalhes
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Box>
        <Pager
          basePath="/suppliers"
          page={list.page}
          pageSize={list.pageSize}
          totalCount={list.totalCount}
          params={{ search, origin, multiBranch, criticality, status }}
        />
      </Card>
    </Stack>
  );
}
