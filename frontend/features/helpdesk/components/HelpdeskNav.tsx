"use client";

import NextLink from "next/link";
import { usePathname } from "next/navigation";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";

const TABS = [
  { href: "/helpdesk", label: "Abrir chamado", match: (path: string) => path === "/helpdesk" || path.startsWith("/helpdesk/new") },
  { href: "/helpdesk/board", label: "Quadro", match: (path: string) => path.startsWith("/helpdesk/board") },
  { href: "/helpdesk/tickets", label: "Chamados", match: (path: string) => path.startsWith("/helpdesk/tickets") },
  { href: "/helpdesk/sectors", label: "Setores", match: (path: string) => path.startsWith("/helpdesk/sectors") },
];

export default function HelpdeskNav() {
  const pathname = usePathname();
  const current = TABS.find((tab) => tab.match(pathname))?.href ?? false;

  return (
    <Tabs value={current} aria-label="Seções do help desk" variant="scrollable" scrollButtons={false} sx={{ minHeight: 40, mb: 2.5, "& .MuiTab-root": { minHeight: 40, py: 0.5 } }}>
      {TABS.map((tab) => (
        <Tab key={tab.href} component={NextLink} href={tab.href} value={tab.href} label={tab.label} />
      ))}
    </Tabs>
  );
}
