import { Suspense, type ReactNode } from "react";
import Box from "@mui/material/Box";
import OeeNav from "@/features/oee/components/OeeNav";

export default function OeeLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Suspense fallback={<Box sx={{ height: 40, mb: 2.5 }} />}>
        <OeeNav />
      </Suspense>
      {children}
    </>
  );
}
