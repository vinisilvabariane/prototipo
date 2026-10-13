"use client";

import { useMemo, useState, type KeyboardEvent } from "react";
import { useRouter } from "next/navigation";
import Search from "@mui/icons-material/Search";
import Box from "@mui/material/Box";
import Dialog from "@mui/material/Dialog";
import InputBase from "@mui/material/InputBase";
import Typography from "@mui/material/Typography";
import { HubIcon } from "@/features/hub/icons";
import { findEntry, isLinkable, type HubEntry } from "@/features/hub/modules";
import { searchEntries } from "@/features/hub/search";
import { recordVisit, useHubPrefs } from "@/features/hub/store";
import { monoFont } from "@/lib/tokens";

type Props = { open: boolean; onClose: () => void };

export default function CommandPalette({ open, onClose }: Props) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      aria-label="Buscar função"
      slotProps={{ paper: { sx: { alignSelf: "flex-start", mt: "12vh" } } }}
    >
      <PaletteBody onClose={onClose} />
    </Dialog>
  );
}

function PaletteBody({ onClose }: { onClose: () => void }) {
  const router = useRouter();
  const [query, setQuery] = useState("");
  const [cursor, setCursor] = useState(0);

  const { recent } = useHubPrefs();
  const recentEntries = recent.flatMap((href) => findEntry(href) ?? []);
  const showingRecent = query.trim() === "" && recentEntries.length > 0;

  const searched = useMemo(() => searchEntries(query, 12), [query]);
  const results = showingRecent ? recentEntries : searched;
  const selectable = results.filter(isLinkable);
  const active = Math.min(cursor, Math.max(selectable.length - 1, 0));
  const activeEntry = selectable[active];

  function open(entry: HubEntry) {
    if (!entry.fn.href) return;
    recordVisit(entry.fn.href);
    onClose();
    router.push(entry.fn.href);
  }

  function handleKeyDown(e: KeyboardEvent) {
    if (selectable.length === 0) return;
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setCursor((active + 1) % selectable.length);
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setCursor((active - 1 + selectable.length) % selectable.length);
    } else if (e.key === "Enter" && activeEntry) {
      e.preventDefault();
      open(activeEntry);
    }
  }

  return (
    <Box>
      <Box sx={{ display: "flex", alignItems: "center", gap: 1.5, px: 2, py: 1.5, borderBottom: 1, borderColor: "divider" }}>
        <Search color="action" />
        <InputBase
          autoFocus
          fullWidth
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
            setCursor(0);
          }}
          onKeyDown={handleKeyDown}
          placeholder="Buscar função ou módulo"
          inputProps={{
            role: "combobox",
            "aria-expanded": true,
            "aria-controls": "palette-results",
            "aria-activedescendant": activeEntry ? `palette-option-${active}` : undefined,
            "aria-label": "Buscar função ou módulo",
          }}
          sx={{ fontSize: "1.05rem" }}
        />
      </Box>

      <Box id="palette-results" role="listbox" sx={{ maxHeight: 360, overflowY: "auto", py: 0.5 }}>
        {showingRecent && (
          <Typography variant="overline" color="text.secondary" sx={{ display: "block", px: 2, pt: 0.5 }}>
            Recentes
          </Typography>
        )}
        {results.length === 0 && (
          <Typography color="text.secondary" sx={{ px: 2, py: 2 }}>
            Nenhuma função encontrada para “{query}”.
          </Typography>
        )}
        {results.map((entry) => {
          const linkable = isLinkable(entry);
          const index = linkable ? selectable.indexOf(entry) : -1;
          const selected = linkable && index === active;
          return (
            <Box
              key={`${entry.module.code}-${entry.fn.label}`}
              id={linkable ? `palette-option-${index}` : undefined}
              role="option"
              aria-selected={selected}
              aria-disabled={!linkable}
              onClick={() => linkable && open(entry)}
              onMouseMove={() => linkable && setCursor(index)}
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 1.5,
                px: 2,
                py: 1,
                cursor: linkable ? "pointer" : "default",
                bgcolor: selected ? "action.selected" : "transparent",
                color: linkable ? "text.primary" : "text.secondary",
              }}
            >
              <Box sx={{ display: "flex", color: linkable ? "text.primary" : "text.disabled" }}>
                <HubIcon name={entry.module.code} />
              </Box>
              <Typography sx={{ flex: 1, fontWeight: linkable ? 500 : 400 }}>{entry.fn.label}</Typography>
              <Typography sx={{ fontSize: "0.8rem", color: "text.secondary" }}>
                {linkable ? entry.group.name : "Em breve"}
              </Typography>
              <Typography sx={{ minWidth: 34, textAlign: "right", fontFamily: monoFont, fontSize: "0.7rem", letterSpacing: "0.08em", color: "text.secondary" }}>
                {entry.module.code}
              </Typography>
            </Box>
          );
        })}
      </Box>

      <Box sx={{ display: "flex", gap: 2, px: 2, py: 1, borderTop: 1, borderColor: "divider", color: "text.secondary", fontSize: "0.75rem" }}>
        <span>↑↓ navegar</span>
        <span>Enter abrir</span>
        <span>Esc fechar</span>
      </Box>
    </Box>
  );
}
