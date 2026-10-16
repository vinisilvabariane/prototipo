import type { Metadata } from "next";
import Box from "@mui/material/Box";
import BrandPanel from "@/features/auth/components/BrandPanel";
import LoginForm from "@/features/auth/components/LoginForm";

export const metadata: Metadata = {
  title: "Entrar",
  description: "Acesse o MeOne com sua matrícula e senha.",
};

export default function LoginPage() {
  return (
    <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", md: "1.15fr 1fr" }, flex: 1 }}>
      <BrandPanel />
      <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", p: 3, bgcolor: "background.paper" }}>
        <LoginForm />
      </Box>
    </Box>
  );
}
