"use client";

import { use, useState, type MouseEvent } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import KeyboardArrowDown from "@mui/icons-material/KeyboardArrowDown";
import Logout from "@mui/icons-material/Logout";
import PersonOutlined from "@mui/icons-material/PersonOutlined";
import Avatar from "@mui/material/Avatar";
import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import Divider from "@mui/material/Divider";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Typography from "@mui/material/Typography";
import { logout } from "@/features/auth/actions/authActions";
import type { SessionUser } from "@/features/auth/types";
import { brand, brandSoft, ink, inkSoft, lineStrong } from "@/lib/tokens";

const initialsOf = (name: string) =>
  name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

export default function UserMenu({ user }: { user: Promise<SessionUser | null> }) {
  const session = use(user);
  const pathname = usePathname();
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const open = Boolean(anchor);

  if (!session || !session.name) return null;

  const role = session.roles[0];
  const onProfile = pathname === "/profile";

  return (
    <>
      <ButtonBase
        onClick={(e: MouseEvent<HTMLElement>) => setAnchor(e.currentTarget)}
        aria-haspopup="menu"
        aria-expanded={open}
        aria-label={`Conta de ${session.name}`}
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 1,
          pl: 0.5,
          pr: 1,
          py: 0.5,
          borderRadius: 999,
          color: ink,
          transition: "background-color 0.2s ease",
          "&:hover": { bgcolor: brandSoft },
          "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2 },
        }}
      >
        <Avatar sx={{ width: 32, height: 32, fontSize: "0.75rem", fontWeight: 600, bgcolor: "transparent", color: ink, border: `1px solid ${lineStrong}` }}>
          {initialsOf(session.name)}
        </Avatar>
        <Box sx={{ display: { xs: "none", lg: "block" }, textAlign: "left", lineHeight: 1.25, whiteSpace: "nowrap" }}>
          <Typography sx={{ fontSize: "0.85rem", fontWeight: 600 }}>{session.name}</Typography>
          {role && (
            <Typography sx={{ fontSize: "0.68rem", fontWeight: 600, letterSpacing: "0.12em", textTransform: "uppercase", color: inkSoft }}>
              {role}
            </Typography>
          )}
        </Box>
        <KeyboardArrowDown
          fontSize="small"
          sx={{ color: inkSoft, transition: "transform 0.2s ease", transform: open ? "rotate(180deg)" : "none" }}
        />
      </ButtonBase>

      <Menu
        anchorEl={anchor}
        open={open}
        onClose={() => setAnchor(null)}
        disableScrollLock
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
        slotProps={{ paper: { sx: { mt: 1, minWidth: 260 } } }}
      >
        <Box sx={{ px: 2, py: 1.5 }}>
          <Typography sx={{ fontWeight: 600 }}>{session.name}</Typography>
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Matrícula{" "}
            <Box component="span" sx={{ fontVariantNumeric: "tabular-nums", color: ink }}>
              {session.registration}
            </Box>
            {session.department ? ` · ${session.department}` : ""}
          </Typography>
        </Box>
        <Divider />
        <MenuItem component={Link} href="/profile" selected={onProfile} onClick={() => setAnchor(null)} sx={{ py: 1.25 }}>
          <ListItemIcon>
            <PersonOutlined fontSize="small" />
          </ListItemIcon>
          <ListItemText primary="Meu perfil" />
        </MenuItem>
        <Divider />
        <form action={logout}>
          <MenuItem component="button" type="submit" sx={{ width: "100%", py: 1.25 }}>
            <ListItemIcon>
              <Logout fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Sair" />
          </MenuItem>
        </form>
      </Menu>
    </>
  );
}
