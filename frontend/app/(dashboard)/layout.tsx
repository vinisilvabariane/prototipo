import { Suspense } from "react";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import Header from "@/components/layout/Header";
import { readSessionUser } from "@/lib/http/session";
import { line } from "@/lib/tokens";

export default function DashboardLayout({ children }: LayoutProps<"/">) {
  const user = readSessionUser();

  return (
    <>
      <Suspense fallback={<Box sx={{ height: 61, borderBottom: `1px solid ${line}` }} />}>
        <Header user={user} />
      </Suspense>
      <Container component="main" maxWidth="xl" sx={{ flex: 1, py: 3, maxWidth: { xl: 1320 } }}>
        {children}
      </Container>
    </>
  );
}
