"use client";

import NextLink from "next/link";
import { usePathname } from "next/navigation";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";

const TABS = [
  { href: "/oee", label: "Comparativo", match: (path: string) => path === "/oee" || path.startsWith("/oee/runs") },
  { href: "/oee/live", label: "Tempo real", match: (path: string) => path.startsWith("/oee/live") },
  { href: "/oee/formulas", label: "Fórmulas", match: (path: string) => path.startsWith("/oee/formulas") },
];

export default function OeeNav() {
  const pathname = usePathname();
  const current = TABS.find((tab) => tab.match(pathname))?.href ?? false;

  return (
    <Tabs value={current} aria-label="Seções do OEE" sx={{ minHeight: 40, mb: 2.5, "& .MuiTab-root": { minHeight: 40, py: 0.5 } }}>
      {TABS.map((tab) => (
        <Tab key={tab.href} component={NextLink} href={tab.href} value={tab.href} label={tab.label} />
      ))}
    </Tabs>
  );
}
