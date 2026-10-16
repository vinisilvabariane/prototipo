"use client";

import type { ReactNode } from "react";
import NextLink from "next/link";
import MuiLink from "@mui/material/Link";
import { brand } from "@/lib/tokens";

export default function TextLink({ href, children }: { href: string; children: ReactNode }) {
  return (
    <MuiLink
      component={NextLink}
      href={href}
      underline="hover"
      sx={{ color: "text.primary", fontWeight: 600, "&:hover": { color: brand } }}
    >
      {children}
    </MuiLink>
  );
}
