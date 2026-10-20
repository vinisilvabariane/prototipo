import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import Stack from "@mui/material/Stack";
import PageError from "@/components/ui/PageError";
import PageHeader from "@/components/ui/PageHeader";
import SupplierForm from "@/features/suppliers/components/SupplierForm";
import { supplierService } from "@/features/suppliers/services/SupplierService";

export const metadata: Metadata = { title: "Editar fornecedor" };

export default async function EditSupplierPage({ params }: PageProps<"/suppliers/[id]/edit">) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id <= 0) notFound();

  const result = await supplierService.getById(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Editar fornecedor" error={result.error} retryHref={`/suppliers/${id}/edit`} />;
  if (!result.value.canEdit) redirect(`/suppliers/${id}`);

  return (
    <Stack spacing={2.5} sx={{ maxWidth: 840 }}>
      <PageHeader title="Editar fornecedor" back={{ href: `/suppliers/${id}`, label: result.value.name }} />
      <SupplierForm supplier={result.value} />
    </Stack>
  );
}
