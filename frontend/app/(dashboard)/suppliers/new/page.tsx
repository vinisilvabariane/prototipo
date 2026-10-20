import type { Metadata } from "next";
import Stack from "@mui/material/Stack";
import PageHeader from "@/components/ui/PageHeader";
import SupplierForm from "@/features/suppliers/components/SupplierForm";

export const metadata: Metadata = { title: "Novo fornecedor" };

export default function NewSupplierPage() {
  return (
    <Stack spacing={2.5} sx={{ maxWidth: 840 }}>
      <PageHeader title="Novo fornecedor" back={{ href: "/suppliers", label: "Fornecedores" }} />
      <SupplierForm />
    </Stack>
  );
}
