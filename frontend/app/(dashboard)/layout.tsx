import Container from "@mui/material/Container";
import Header from "@/components/layout/Header";
import { readSessionUser } from "@/lib/http/session";

export default function DashboardLayout({ children }: LayoutProps<"/">) {
  const user = readSessionUser();

  return (
    <>
      <Header user={user} />
      <Container component="main" maxWidth="xl" sx={{ flex: 1, py: 4 }}>
        {children}
      </Container>
    </>
  );
}
