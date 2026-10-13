"use client";

import Link from "next/link";
import PushPin from "@mui/icons-material/PushPin";
import PushPinOutlined from "@mui/icons-material/PushPinOutlined";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import IconButton from "@mui/material/IconButton";
import { keyframes } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import { brand, brandDeep, brandSoft, displayFont, easeOut, inkSoft, line, monoFont } from "@/lib/tokens";
import type { HubModule } from "../modules";
import { MAX_PINNED, recordVisit, togglePin, useHubPrefs } from "../store";

const rise = keyframes`
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: none; }
`;

const step = (index: number) => ({
  "@media (prefers-reduced-motion: no-preference)": { animation: `${rise} 0.6s ${easeOut} ${index * 55}ms backwards` },
});

type Props = {
  module: HubModule;
  groupName: string;
  counts: Record<string, number> | null;
};

export default function ModuleSheet({ module, groupName, counts }: Props) {
  const { pinned } = useHubPrefs();
  const active = module.status === "active";
  const pinsFull = pinned.length >= MAX_PINNED;

  return (
    <Card>
      <Box key={module.code} sx={{ p: 3 }}>
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 1, ...step(0) }}>
          <Typography variant="overline" sx={{ color: inkSoft }}>
            {groupName}
          </Typography>
          <Box
            component="span"
            sx={{
              px: 1.25,
              py: 0.35,
              borderRadius: 999,
              fontSize: "0.74rem",
              fontWeight: 600,
              bgcolor: active ? brandSoft : "transparent",
              color: active ? brandDeep : inkSoft,
              border: `1px solid ${active ? "transparent" : line}`,
            }}
          >
            {active ? "Em operação" : "Em breve"}
          </Box>
        </Box>

        <Typography sx={{ mt: 2.5, fontFamily: monoFont, fontSize: "0.78rem", letterSpacing: "0.1em", color: inkSoft, ...step(1) }}>
          {module.code}
        </Typography>
        <Typography
          component="h2"
          sx={{ fontFamily: displayFont, fontWeight: 600, fontSize: "1.7rem", lineHeight: 1.1, letterSpacing: "-0.03em", mt: 0.5, ...step(2) }}
        >
          {module.name}
        </Typography>
        <Typography sx={{ mt: 1.5, color: inkSoft, ...step(3) }}>{module.description}</Typography>

        <Box component="ul" sx={{ listStyle: "none", m: 0, mt: 3, p: 0, borderTop: `1px solid ${line}` }}>
          {module.functions.map((fn, index) => {
            const href = active ? fn.href : undefined;
            const slot = href ? pinned.indexOf(href) + 1 : 0;

            return (
              <Box
                key={fn.label}
                component="li"
                sx={{ display: "flex", alignItems: "center", gap: 1, borderBottom: `1px solid ${line}`, ...step(4 + index) }}
              >
                {href ? (
                  <Box
                    component={Link}
                    href={href}
                    onClick={() => recordVisit(href)}
                    sx={{
                      flex: 1,
                      py: 1.5,
                      color: "text.primary",
                      textDecoration: "none",
                      fontWeight: 600,
                      transition: `color 0.2s ease, transform 0.35s ${easeOut}`,
                      "&:hover": { color: brand, transform: "translateX(4px)" },
                      "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2, borderRadius: "4px" },
                      "@media (prefers-reduced-motion: reduce)": { "&:hover": { transform: "none" } },
                    }}
                  >
                    {fn.label}
                  </Box>
                ) : (
                  <Typography sx={{ flex: 1, py: 1.5, color: inkSoft }}>{fn.label}</Typography>
                )}

                {href && counts?.[href] !== undefined && (
                  <Box component="span" sx={{ fontFamily: monoFont, fontSize: "0.82rem", color: inkSoft }}>
                    {counts[href]}
                  </Box>
                )}

                {href && (
                  <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
                    {slot > 0 && (
                      <Box component="kbd" sx={{ fontFamily: monoFont, fontSize: "0.7rem", px: 0.75, border: `1px solid ${line}`, borderRadius: "6px", color: inkSoft }}>
                        {slot}
                      </Box>
                    )}
                    <IconButton
                      size="small"
                      aria-pressed={slot > 0}
                      aria-label={
                        slot > 0
                          ? `Remover ${fn.label} das teclas de atalho`
                          : pinsFull
                            ? "Todas as 8 teclas de atalho estão em uso"
                            : `Fixar ${fn.label} numa tecla de atalho`
                      }
                      disabled={slot === 0 && pinsFull}
                      onClick={() => togglePin(href)}
                      sx={{ color: slot > 0 ? brand : inkSoft }}
                    >
                      {slot > 0 ? <PushPin sx={{ fontSize: 18 }} /> : <PushPinOutlined sx={{ fontSize: 18 }} />}
                    </IconButton>
                  </Box>
                )}
              </Box>
            );
          })}
        </Box>

        {!active && (
          <Typography variant="body2" sx={{ mt: 2, color: inkSoft, ...step(8) }}>
            Este módulo ainda não foi liberado. As funções acima mostram o que ele vai cobrir.
          </Typography>
        )}
      </Box>
    </Card>
  );
}
