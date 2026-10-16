import type { Metadata } from "next";
import { notFound } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import { CompanyDocumentsSection, MaterialsSection, ToolsSection } from "@/features/suppliers/components/CatalogSections";
import { AuditsSection, QualificationSection, SelfAssessmentsSection } from "@/features/suppliers/components/EvaluationSections";
import { Field, Section } from "@/features/suppliers/components/Parts";
import { CriticalityChip, QualificationChip } from "@/features/suppliers/components/SupplierBadges";
import { formatCnpj, formatDateTime } from "@/features/suppliers/format";
import { MULTI_BRANCH_LABELS, ORIGIN_LABELS } from "@/features/suppliers/labels";
import { supplierService } from "@/features/suppliers/services/SupplierService";
import { inkSoft } from "@/lib/tokens";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function generateMetadata({ params }: PageProps<"/suppliers/[id]">): Promise<Metadata> {
  const id = parseId((await params).id);
  if (id === null) return { title: "Fornecedor não encontrado" };

  const result = await supplierService.getById(id);
  if (result.ok) return { title: `${result.value.sapCode} · ${result.value.name}` };
  return { title: result.error.code === "notFound" ? "Fornecedor não encontrado" : "Fornecedor" };
}

export default async function SupplierPage({ params }: PageProps<"/suppliers/[id]">) {
  const id = parseId((await params).id);
  if (id === null) notFound();

  const result = await supplierService.getById(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Fornecedor" error={result.error} retryHref={`/suppliers/${id}`} />;

  const supplier = result.value;

  const users = supplier.canEdit ? await supplierService.users() : null;
  if (users && !users.ok) return <PageError title="Fornecedor" error={users.error} retryHref={`/suppliers/${id}`} />;
  const responsibles = users?.value ?? [];

  const address = [supplier.street && `${supplier.street}${supplier.number ? `, ${supplier.number}` : ""}`, supplier.district, supplier.zipCode].filter(Boolean).join(" · ");

  return (
    <Stack spacing={4}>
      <Box>
        <Button href="/suppliers" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
          Fornecedores
        </Button>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ mt: 1, justifyContent: "space-between", alignItems: { md: "flex-start" } }}>
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="overline" component="p" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
              {supplier.sapCode} · {MULTI_BRANCH_LABELS[supplier.multiBranch]}
            </Typography>
            <Typography variant="h4" component="h1" sx={{ overflowWrap: "anywhere" }}>
              {supplier.name}
            </Typography>
            <Stack direction="row" spacing={1} sx={{ mt: 1.5, alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
              <QualificationChip qualification={supplier.qualification} />
              <CriticalityChip criticality={supplier.criticality} />
            </Stack>
          </Box>
          {supplier.canEdit && (
            <Button variant="outlined" href={`/suppliers/${id}/edit`} sx={{ alignSelf: { md: "flex-start" } }}>
              Editar dados
            </Button>
          )}
        </Stack>
      </Box>

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 1fr) minmax(0, 1fr)" }, alignItems: "start" }}>
        <Section title="Dados do fornecedor">
          <Box>
            <Field label="Razão social">{supplier.corporateName ?? "—"}</Field>
            <Field label="Filial">{supplier.branch}</Field>
            <Field label="Origem">{ORIGIN_LABELS[supplier.origin]}</Field>
            <Field label={supplier.origin === "domestic" ? "CNPJ" : "Documento fiscal"}>{supplier.origin === "domestic" ? formatCnpj(supplier.taxId) : (supplier.taxId ?? "—")}</Field>
            <Field label="Endereço">{address}</Field>
            <Field label="Responsável">{supplier.contactName ?? "—"}</Field>
            <Field label="Telefone">{supplier.phone ?? "—"}</Field>
            <Field label="E-mail">{supplier.email ?? "—"}</Field>
            <Field label="Cadastrado por">
              {supplier.createdBy} · {formatDateTime(supplier.createdAt)}
            </Field>
            <Field label="Atualizado por">
              {supplier.updatedBy} · {formatDateTime(supplier.updatedAt)}
            </Field>
          </Box>
        </Section>

        <QualificationSection supplier={supplier} />
      </Box>

      <CompanyDocumentsSection supplier={supplier} />
      <SelfAssessmentsSection supplier={supplier} users={responsibles} />
      <AuditsSection supplier={supplier} users={responsibles} />
      <ToolsSection supplier={supplier} />
      <MaterialsSection supplier={supplier} />
    </Stack>
  );
}
