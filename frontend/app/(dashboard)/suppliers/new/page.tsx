import type { Metadata } from "next";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import SupplierForm from "@/features/suppliers/components/SupplierForm";
import { inkSoft } from "@/lib/tokens";

export const metadata: Metadata = { title: "Novo fornecedor" };

export default function NewSupplierPage() {
  return (
    <Box sx={{ maxWidth: 840 }}>
      <Button href="/suppliers" size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1 }}>
        Fornecedores
      </Button>
      <Typography variant="h4" component="h1" sx={{ mt: 1, mb: 3 }}>
        Novo fornecedor
      </Typography>
      <SupplierForm />
    </Box>
  );
}
