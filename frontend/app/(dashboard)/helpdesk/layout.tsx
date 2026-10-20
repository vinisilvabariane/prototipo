import { Suspense, type ReactNode } from "react";
import Box from "@mui/material/Box";
import HelpdeskNav from "@/features/helpdesk/components/HelpdeskNav";

export default function HelpdeskLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Suspense fallback={<Box sx={{ height: 40, mb: 2.5 }} />}>
        <HelpdeskNav />
      </Suspense>
      {children}
    </>
  );
}
