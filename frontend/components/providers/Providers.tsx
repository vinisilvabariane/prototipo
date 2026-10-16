"use client";

import type { ReactNode } from "react";
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import LibrasWidget from "@/features/settings/components/LibrasWidget";
import PrefsApplier from "@/features/settings/components/PrefsApplier";
import { theme } from "@/lib/theme";

export default function Providers({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider theme={theme} defaultMode="light" disableTransitionOnChange>
      <CssBaseline />
      <PrefsApplier />
      <LibrasWidget />
      {children}
    </ThemeProvider>
  );
}
