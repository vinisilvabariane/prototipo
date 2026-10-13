"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import Search from "@mui/icons-material/Search";
import Box from "@mui/material/Box";
import InputAdornment from "@mui/material/InputAdornment";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { keyframes } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { brand, easeOut, inkSoft, line, lineStrong, monoFont } from "@/lib/tokens";
import { allModules, availableModules, hubGroups, isLinkable } from "../modules";
import { moduleMatches, searchEntries } from "../search";
import { MAX_PINNED, recordVisit, useHubPrefs } from "../store";
import ModulePlate from "./ModulePlate";
import ModuleSheet from "./ModuleSheet";

const rise = keyframes`
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: none; }
`;

const enter = (index: number) => ({
  "@media (prefers-reduced-motion: no-preference)": { animation: `${rise} 0.7s ${easeOut} ${index * 70}ms backwards` },
});

type Props = {
  counts: Record<string, number> | null;
  error: AppError | null;
};

function isTyping(target: EventTarget | null) {
  const element = target as HTMLElement | null;
  return !!element && (element.tagName === "INPUT" || element.tagName === "TEXTAREA" || element.isContentEditable);
}

export default function HubBoard({ counts, error }: Props) {
  const router = useRouter();
  const { pinned } = useHubPrefs();
  const [query, setQuery] = useState("");
  const [selected, setSelected] = useState(availableModules[0]?.code ?? allModules[0].code);
  const inputRef = useRef<HTMLInputElement>(null);
  const sheetRef = useRef<HTMLDivElement>(null);

  const search = query.trim();
  const matches = useMemo(
    () => (search ? new Set(allModules.filter((module) => moduleMatches(module, search)).map((module) => module.code)) : null),
    [search],
  );
  const firstMatch = matches ? allModules.find((module) => matches.has(module.code)) : undefined;
  const shownCode = matches && !matches.has(selected) && firstMatch ? firstMatch.code : selected;
  const shownModule = allModules.find((module) => module.code === shownCode) ?? allModules[0];
  const shownGroup = hubGroups.find((group) => group.modules.includes(shownModule)) ?? hubGroups[0];

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.ctrlKey || e.metaKey || e.altKey || isTyping(e.target)) return;

      if (e.key === "/") {
        e.preventDefault();
        inputRef.current?.focus();
        return;
      }

      const slot = Number(e.key);
      const href = slot >= 1 && slot <= MAX_PINNED ? pinned[slot - 1] : undefined;
      if (href) {
        recordVisit(href);
        router.push(href);
      }
    }
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [pinned, router]);

  function selectModule(code: string) {
    setSelected(code);
    const small = window.matchMedia("(max-width: 899.95px)").matches;
    const reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (small) sheetRef.current?.scrollIntoView({ behavior: reduced ? "auto" : "smooth", block: "start" });
  }

  function openFirstResult() {
    const first = searchEntries(search).find(isLinkable);
    if (first?.fn.href) {
      recordVisit(first.fn.href);
      router.push(first.fn.href);
    }
  }

  return (
    <Stack spacing={4}>
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", md: "row" },
          justifyContent: "space-between",
          alignItems: { md: "flex-end" },
          gap: 2,
        }}
      >
        <Box>
          <Typography variant="h4" component="h1" sx={enter(0)}>
            Módulos
          </Typography>
          <Typography sx={{ mt: 0.75, color: inkSoft, ...enter(1) }}>
            <Box component="span" sx={{ color: "text.primary", fontWeight: 600 }}>
              {availableModules.length}
            </Box>{" "}
            em operação · {allModules.length - availableModules.length} em breve
          </Typography>
        </Box>

        <Box sx={{ display: "flex", alignItems: "center", gap: 1, color: inkSoft, fontSize: "0.82rem", ...enter(1) }}>
          <Box
            aria-hidden
            sx={{ width: 8, height: 8, borderRadius: "50%", border: `1.5px solid ${brand}`, bgcolor: error ? "transparent" : brand }}
          />
          {error ? "API indisponível" : "API online"}
        </Box>
      </Box>

      {error && <ErrorAlert error={error} retryHref="/hub" />}

      <Box sx={enter(2)}>
        <TextField
          inputRef={inputRef}
          variant="standard"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Escape") setQuery("");
            if (e.key === "Enter") openFirstResult();
          }}
          placeholder="Localizar módulo ou função"
          fullWidth
          sx={{
            maxWidth: 520,
            "& .MuiInputBase-root": { fontSize: "1.05rem", py: 0.5 },
            "& .MuiInput-underline:before": { borderBottomColor: lineStrong },
            "& .MuiInput-underline:hover:not(.Mui-disabled):before": { borderBottomColor: brand },
            "& .MuiInput-underline:after": { borderBottomColor: brand },
          }}
          slotProps={{
            htmlInput: { "aria-label": "Localizar módulo ou função" },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search sx={{ color: inkSoft }} />
                </InputAdornment>
              ),
              endAdornment: (
                <InputAdornment position="end">
                  <Box
                    component="kbd"
                    aria-hidden
                    sx={{ fontFamily: monoFont, fontSize: "0.72rem", px: 0.75, border: `1px solid ${line}`, borderRadius: "6px", color: inkSoft }}
                  >
                    /
                  </Box>
                </InputAdornment>
              ),
            },
          }}
        />

        {matches && matches.size === 0 && (
          <Typography sx={{ mt: 2, color: inkSoft }}>
            Nenhum módulo corresponde a “{search}”. Busque pelo nome, pelo código (ex.: PCP) ou por uma função.
          </Typography>
        )}
      </Box>

      <Box
        sx={{
          display: "grid",
          gap: { xs: 4, md: 6 },
          alignItems: "start",
          gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 1fr) 380px" },
        }}
      >
        <ModulePlate selected={shownCode} matches={matches} onSelect={selectModule} />
        <Box ref={sheetRef} sx={{ position: { md: "sticky" }, top: { md: 88 }, scrollMarginTop: 88 }}>
          <ModuleSheet module={shownModule} groupName={shownGroup.name} counts={counts} />
        </Box>
      </Box>
    </Stack>
  );
}
