"use client";

import { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import Search from "@mui/icons-material/Search";
import AppBar from "@mui/material/AppBar";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import LogoMark from "@/components/ui/LogoMark";
import { logout } from "@/features/auth/actions/authActions";
import type { SessionUser } from "@/features/auth/types";
import { brand, displayFont, inkSoft, line, monoFont } from "@/lib/tokens";
import CommandPalette from "./CommandPalette";
import LiveClock from "./LiveClock";
import UserBadge from "./UserBadge";

const accessLinks = [
  { href: "/users", label: "Usuários" },
  { href: "/roles", label: "Perfis" },
  { href: "/departments", label: "Departamentos" },
];

export default function Header({ user }: { user: Promise<SessionUser | null> }) {
  const pathname = usePathname();
  const [paletteOpen, setPaletteOpen] = useState(false);
  const inAccessModule = accessLinks.some((link) => pathname.startsWith(link.href));

  useEffect(() => {
    function openOnCtrlK(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        setPaletteOpen(true);
      }
    }
    document.addEventListener("keydown", openOnCtrlK);
    return () => document.removeEventListener("keydown", openOnCtrlK);
  }, []);

  return (
    <AppBar
      position="sticky"
      color="inherit"
      elevation={0}
      sx={{
        bgcolor: "rgba(255, 255, 255, 0.85)",
        backdropFilter: "blur(10px)",
        borderBottom: `1px solid ${line}`,
        color: "text.primary",
      }}
    >
      <Toolbar sx={{ gap: 2.5, minHeight: 60 }}>
        <Box component={Link} href="/hub" sx={{ display: "flex", alignItems: "center", gap: 1.25, color: "inherit", textDecoration: "none" }}>
          <LogoMark />
          <Typography sx={{ fontFamily: displayFont, fontWeight: 600, fontSize: "1.2rem", letterSpacing: "-0.03em" }}>
            MeOne
          </Typography>
        </Box>

        <Button component={Link} href="/hub" color="inherit" size="small" aria-current={pathname === "/hub" ? "page" : undefined} sx={{ color: pathname === "/hub" ? "text.primary" : inkSoft }}>
          Hub
        </Button>

        {inAccessModule && (
          <Box sx={{ display: { xs: "none", md: "flex" }, alignItems: "center", gap: 1, minWidth: 0 }}>
            <Typography component="span" sx={{ color: inkSoft }}>
              /
            </Typography>
            <Typography component="span" sx={{ display: { xs: "none", lg: "inline" }, fontWeight: 600, fontSize: "0.85rem", whiteSpace: "nowrap", mr: 1 }}>
              Controle de acesso
            </Typography>
            {accessLinks.map((link) => {
              const current = pathname.startsWith(link.href);
              return (
                <Button
                  key={link.href}
                  component={Link}
                  href={link.href}
                  color="inherit"
                  size="small"
                  aria-current={current ? "page" : undefined}
                  sx={{
                    borderRadius: 0,
                    borderBottom: 2,
                    borderBottomColor: current ? brand : "transparent",
                    color: current ? "text.primary" : inkSoft,
                  }}
                >
                  {link.label}
                </Button>
              );
            })}
          </Box>
        )}

        <Box sx={{ flexGrow: 1 }} />

        <Button
          color="inherit"
          size="small"
          onClick={() => setPaletteOpen(true)}
          aria-label="Buscar função (Ctrl K)"
          startIcon={<Search fontSize="small" />}
          sx={{ color: inkSoft, gap: 0.5, minWidth: 0, flexShrink: 0, whiteSpace: "nowrap" }}
        >
          <Box component="span" sx={{ display: { xs: "none", sm: "inline" } }}>
            Buscar
          </Box>
          <Box
            component="kbd"
            aria-hidden
            sx={{ display: { xs: "none", sm: "inline" }, ml: 1, fontFamily: monoFont, fontSize: "0.7rem", px: 0.75, border: `1px solid ${line}`, borderRadius: "6px", whiteSpace: "nowrap" }}
          >
            Ctrl K
          </Box>
        </Button>

        <LiveClock />

        <Suspense fallback={<Box sx={{ width: 32, height: 32, borderRadius: "50%", border: `1px solid ${line}` }} />}>
          <UserBadge user={user} />
        </Suspense>

        <form action={logout}>
          <Button type="submit" color="inherit" size="small" sx={{ color: inkSoft }}>
            Sair
          </Button>
        </form>
      </Toolbar>

      <CommandPalette open={paletteOpen} onClose={() => setPaletteOpen(false)} />
    </AppBar>
  );
}
