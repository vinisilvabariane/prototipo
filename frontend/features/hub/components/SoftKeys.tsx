"use client";

import Link from "next/link";
import Close from "@mui/icons-material/Close";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import { brand, brandSoft, ink, inkSoft, line, lineStrong, monoFont } from "@/lib/tokens";
import { findEntry } from "../modules";
import { MAX_PINNED, recordVisit, togglePin, useHubPrefs } from "../store";

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
        color: "#FFFFFF",
        fontFamily: monoFont,
        fontSize: "0.72rem",
        fontWeight: 500,
      }}
    >
      {slot}
    </Box>
  );
}

function SoftKey({ slot, href }: { slot: number; href: string | undefined }) {
  const entry = href ? findEntry(href) : undefined;

  if (!href || !entry) {
    return (
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 1,
          height: 44,
          px: 1,
          border: "1px dashed",
          borderColor: lineStrong,
          borderRadius: "12px",
          color: inkSoft,
          fontSize: "0.8rem",
        }}
      >
        <Box component="kbd" sx={{ width: 22, textAlign: "center", fontFamily: monoFont, fontSize: "0.72rem" }}>
          {slot}
        </Box>
        livre
      </Box>
    );
  }

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
          height: 44,
          pl: 1,
          pr: 4,
          borderRadius: "12px",
          border: `1px solid ${line}`,
          color: ink,
          textDecoration: "none",
          fontSize: "0.85rem",
          fontWeight: 600,
          transition: "background-color 0.2s ease, border-color 0.2s ease",
          "&:hover": { bgcolor: brandSoft, borderColor: brand },
          "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2 },
        }}
      >
        <Keycap slot={slot} />
        <Box component="span" sx={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
          {entry.fn.label}
        </Box>
      </Box>
      <IconButton
        className="unpin"
        size="small"
        aria-label={`Remover ${entry.fn.label} das teclas de atalho`}
        onClick={() => togglePin(href)}
        sx={{
          position: "absolute",
          top: 10,
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
    <Box>
      <Typography variant="overline" component="h2" sx={{ display: "block", color: "text.primary" }}>
        Teclas de atalho
      </Typography>
      <Box
        sx={{
          display: "grid",
          gap: 1.25,
          mt: 1.5,
          gridTemplateColumns: { xs: "repeat(2, 1fr)", sm: "repeat(4, 1fr)", xl: "repeat(8, 1fr)" },
        }}
      >
        {Array.from({ length: MAX_PINNED }, (_, index) => (
          <SoftKey key={index} slot={index + 1} href={pinned[index]} />
        ))}
      </Box>
      {pinned.length === 0 && (
        <Typography variant="caption" sx={{ display: "block", mt: 1.5, color: inkSoft }}>
          Abra a ficha de um módulo e fixe uma função para criar um atalho. Depois, aperte o número da tecla em qualquer ponto do quadro.
        </Typography>
      )}
    </Box>
  );
}
