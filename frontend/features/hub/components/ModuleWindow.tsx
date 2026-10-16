"use client";

import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import { keyframes } from "@mui/material/styles";
import { brand, brandDeep, brandWash, easeOut, ink, inkSoft, line, onInk, onInkSoft } from "@/lib/tokens";
import type { HubModule } from "../modules";

const fadeIn = keyframes`
  from { opacity: 0; }
  to { opacity: 1; }
`;

const liftIn = keyframes`
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: none; }
`;

type Props = {
  module: HubModule;
  index: number;
  selected: boolean;
  dimmed: boolean;
  onSelect: () => void;
};

export default function ModuleWindow({ module, index, selected, dimmed, onSelect }: Props) {
  const active = module.status === "active";
  const delay = index * 35;

  return (
    <ButtonBase
      data-code={module.code}
      onClick={onSelect}
      aria-pressed={selected}
      aria-label={`${module.code}: ${module.name}, ${active ? "em operação" : "em breve"}`}
      sx={{
        position: "relative",
        overflow: "hidden",
        width: "100%",
        minHeight: 96,
        display: "flex",
        flexDirection: "column",
        alignItems: "flex-start",
        justifyContent: "space-between",
        p: "14px 16px",
        textAlign: "left",
        borderRadius: "14px",
        border: `1px solid ${active ? ink : line}`,
        bgcolor: active ? ink : "transparent",
        color: active ? onInk : ink,
        opacity: dimmed ? 0.22 : 1,
        transition: "opacity 0.3s ease",
        "&::before": {
          content: '""',
          position: "absolute",
          inset: 0,
          bgcolor: active ? brandDeep : brandWash,
          transform: "scaleX(0)",
          transformOrigin: "left",
          transition: `transform 0.5s ${easeOut}`,
        },
        "&:hover::before, &:focus-visible::before": { transform: "scaleX(1)" },
        "& > *": { position: "relative" },
        "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 4 },
        "@media (prefers-reduced-motion: reduce)": { "&::before": { transition: "none" } },
        "@media (prefers-reduced-motion: no-preference)": {
          animation: `${fadeIn} 0.6s ease ${delay}ms backwards`,
          "& > *": { animation: `${liftIn} 0.7s ${easeOut} ${delay + 80}ms backwards` },
        },
      }}
    >
      <Box sx={{ fontWeight: 600, fontSize: "1.5rem", lineHeight: 1, letterSpacing: "-0.02em" }}>
        {module.code}
      </Box>
      <Box
        sx={{
          fontSize: "0.76rem",
          fontWeight: 500,
          lineHeight: 1.25,
          color: active ? onInkSoft : inkSoft,
          display: "-webkit-box",
          WebkitLineClamp: 3,
          WebkitBoxOrient: "vertical",
          overflow: "hidden",
        }}
      >
        {module.name}
      </Box>
    </ButtonBase>
  );
}
