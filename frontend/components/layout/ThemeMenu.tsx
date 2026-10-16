"use client";

import { useState, type MouseEvent } from "react";
import Check from "@mui/icons-material/Check";
import Palette from "@mui/icons-material/Palette";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Tooltip from "@mui/material/Tooltip";
import { setPref, usePrefs } from "@/lib/prefs/store";
import { inkSoft, THEME_NAMES, themes, type ThemeName } from "@/lib/tokens";

function Swatch({ name }: { name: ThemeName }) {
  const { tokens } = themes[name];
  return (
    <Box
      aria-hidden
      sx={{
        width: 22,
        height: 22,
        borderRadius: "50%",
        border: `1px solid ${tokens.lineStrong}`,
        background: `linear-gradient(135deg, ${tokens.canvas} 50%, ${tokens.brand} 50%)`,
      }}
    />
  );
}

export default function ThemeMenu() {
  const { theme } = usePrefs();
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const open = Boolean(anchor);

  function choose(name: ThemeName) {
    setPref("theme", name);
    setAnchor(null);
  }

  return (
    <>
      <Tooltip title={`Tema: ${themes[theme].label}`}>
        <IconButton
          size="small"
          aria-label={`Tema: ${themes[theme].label}. Trocar tema`}
          aria-haspopup="menu"
          aria-expanded={open}
          onClick={(e: MouseEvent<HTMLElement>) => setAnchor(e.currentTarget)}
          sx={{ color: inkSoft }}
        >
          <Palette fontSize="small" />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchor}
        open={open}
        onClose={() => setAnchor(null)}
        disableScrollLock
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
        slotProps={{ paper: { sx: { mt: 1, minWidth: 260 } } }}
      >
        {THEME_NAMES.map((name) => (
          <MenuItem key={name} selected={name === theme} onClick={() => choose(name)} sx={{ py: 1 }}>
            <ListItemIcon>
              <Swatch name={name} />
            </ListItemIcon>
            <ListItemText primary={themes[name].label} secondary={themes[name].hint} />
            {name === theme && <Check fontSize="small" sx={{ ml: 2 }} />}
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
