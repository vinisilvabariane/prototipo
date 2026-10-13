"use client";

import { use } from "react";
import Avatar from "@mui/material/Avatar";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import type { SessionUser } from "@/features/auth/types";
import { ink, inkSoft, lineStrong } from "@/lib/tokens";

export default function UserBadge({ user }: { user: Promise<SessionUser | null> }) {
  const session = use(user);
  if (!session || !session.name) return null;

  const initials = session.name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
  const role = session.roles[0];

  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1.25 }}>
      <Box sx={{ display: { xs: "none", lg: "block" }, textAlign: "right", lineHeight: 1.25, whiteSpace: "nowrap" }}>
        <Typography sx={{ fontSize: "0.85rem", fontWeight: 600 }}>{session.name}</Typography>
        {role && (
          <Typography sx={{ fontSize: "0.68rem", fontWeight: 600, letterSpacing: "0.12em", textTransform: "uppercase", color: inkSoft }}>
            {role}
          </Typography>
        )}
      </Box>
      <Avatar sx={{ width: 32, height: 32, fontSize: "0.75rem", fontWeight: 600, bgcolor: "transparent", color: ink, border: `1px solid ${lineStrong}` }}>
        {initials}
      </Avatar>
    </Box>
  );
}
