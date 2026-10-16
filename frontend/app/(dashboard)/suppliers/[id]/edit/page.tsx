import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import PageError from "@/components/ui/PageError";
import SupplierForm from "@/features/suppliers/components/SupplierForm";
import { supplierService } from "@/features/suppliers/services/SupplierService";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = { title: "Editar fornecedor" };

export default async function EditSupplierPage({ params }: PageProps<"/suppliers/[id]/edit">) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id <= 0) notFound();

  const result = await supplierService.getById(id);
  if (!result.ok && result.error.code === "notFound") notFound();
  if (!result.ok) return <PageError title="Editar fornecedor" error={result.error} retryHref={`/suppliers/${id}/edit`} />;
  if (!result.value.canEdit) redirect(`/suppliers/${id}`);

  return (
    <Box sx={{ maxWidth: 840 }}>
      <Button href={`/suppliers/${id}`} size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
        {result.value.name}
      </Button>
      <Typography variant="h4" component="h1" sx={{ mt: 1, mb: 3 }}>
        Editar fornecedor
      </Typography>
      <SupplierForm supplier={result.value} />
    </Box>
  );
}
