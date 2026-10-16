"use client";

import Link from "next/link";
import Close from "@mui/icons-material/Close";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import { brand, brandSoft, ink, inkSoft, line, lineStrong, onInk } from "@/lib/tokens";
import { HubIcon } from "../icons";
import { findEntry } from "../modules";
import { HOTKEYS, recordVisit, togglePin, useHubPrefs } from "../store";

function Keycap({ slot }: { slot: number }) {
  return (
    <Box
      component="kbd"
      sx={{
        display: "grid",
        placeItems: "center",
        flexShrink: 0,
        width: 22,
        height: 22,
        borderRadius: "6px",
        bgcolor: ink,
        color: onInk,
        fontVariantNumeric: "tabular-nums",
        fontSize: "0.72rem",
        fontWeight: 500,
      }}
    >
      {slot}
    </Box>
  );
}

function SoftKey({ href, slot }: { href: string; slot?: number }) {
  const entry = findEntry(href);
  if (!entry) return null;

  return (
    <Box sx={{ position: "relative", "&:hover .unpin, &:focus-within .unpin": { opacity: 1 } }}>
      <Box
        component={Link}
        href={href}
        onClick={() => recordVisit(href)}
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 1,
          height: 42,
          pl: slot ? 1 : 1.5,
          pr: 4,
          borderRadius: "12px",
          border: `1px solid ${line}`,
          borderBottom: `3px solid ${lineStrong}`,
          color: ink,
          textDecoration: "none",
          fontSize: "0.85rem",
          fontWeight: 600,
          transition: "background-color 0.2s ease, border-color 0.2s ease",
          "&:hover": { bgcolor: brandSoft, borderColor: brand },
          "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2 },
        }}
      >
        {slot ? <Keycap slot={slot} /> : <HubIcon name={entry.module.code} sx={{ color: inkSoft }} />}
        <Box component="span" sx={{ whiteSpace: "nowrap" }}>
          {entry.fn.label}
        </Box>
      </Box>
      <IconButton
        className="unpin"
        size="small"
        aria-label={`Remover ${entry.fn.label} dos atalhos`}
        onClick={() => togglePin(href)}
        sx={{
          position: "absolute",
          top: 9,
          right: 6,
          p: 0.25,
          color: inkSoft,
          opacity: 0,
          "@media (hover: none)": { opacity: 1 },
          "&:focus-visible": { opacity: 1 },
        }}
      >
        <Close sx={{ fontSize: 14 }} />
      </IconButton>
    </Box>
  );
}

export default function SoftKeys() {
  const { pinned } = useHubPrefs();

  return (
    <Box component="section" aria-labelledby="softkeys-title">
      <Box sx={{ display: "flex", alignItems: "baseline", gap: 2, flexWrap: "wrap" }}>
        <Typography id="softkeys-title" variant="overline" component="h2" sx={{ color: "text.primary" }}>
          Atalhos
        </Typography>
        {pinned.length > 0 && (
          <Typography variant="caption" sx={{ color: inkSoft }}>
            Os {Math.min(pinned.length, HOTKEYS)} primeiros abrem pelas teclas 1 a {Math.min(pinned.length, HOTKEYS)}.
          </Typography>
        )}
      </Box>

      {pinned.length === 0 ? (
        <Typography variant="body2" sx={{ mt: 1, px: 2, py: 1.5, border: `1px dashed ${lineStrong}`, borderRadius: "12px", color: inkSoft }}>
          Abra a ficha de um módulo e clique no alfinete de uma função para criar um atalho aqui.
        </Typography>
      ) : (
        <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1.25, mt: 1.5 }}>
          {pinned.map((href, index) => (
            <SoftKey key={href} href={href} slot={index < HOTKEYS ? index + 1 : undefined} />
          ))}
        </Box>
      )}
    </Box>
  );
}
