"use client";

import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { keyframes } from "@mui/material/styles";
import LogoMark from "@/components/ui/LogoMark";
import { allModules, availableModules } from "@/features/hub/modules";
import { displayFont, ink } from "@/lib/tokens";

const COLUMNS = 5;

const breathe = keyframes`
  0%, 100% { opacity: 0.35; }
  50% { opacity: 0.85; }
`;

export default function BrandPanel() {
  return (
    <Box
      sx={{
        position: "relative",
        overflow: "hidden",
        display: { xs: "none", md: "flex" },
        flexDirection: "column",
        justifyContent: "space-between",
        gap: 5,
        p: 6,
        color: "#FFFFFF",
        bgcolor: ink,
        backgroundImage: "radial-gradient(700px 460px at 100% 0%, rgba(37, 71, 208, 0.45), transparent 65%)",
      }}
    >
      <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
        <LogoMark size={26} tone="dark" />
        <Typography sx={{ fontFamily: displayFont, fontWeight: 600, fontSize: "1.5rem", letterSpacing: "-0.03em" }}>
          MeOne
        </Typography>
      </Box>

      <Box>
        <Typography variant="h4" component="p" sx={{ maxWidth: 420, mb: 4 }}>
          Um só lugar para operar a planta.
        </Typography>

        <Box aria-hidden sx={{ display: "grid", gridTemplateColumns: `repeat(${COLUMNS}, 1fr)`, gap: 1, maxWidth: 520 }}>
          {allModules.map((module, index) => {
            const active = module.status === "active";
            const wave = ((index % COLUMNS) + Math.floor(index / COLUMNS)) * 160;
            return (
              <Box
                key={module.code}
                sx={{
                  display: "grid",
                  placeItems: "center",
                  aspectRatio: "1.35",
                  borderRadius: "12px",
                  border: "1px solid",
                  borderColor: active ? "#FFFFFF" : "rgba(255, 255, 255, 0.2)",
                  bgcolor: active ? "#FFFFFF" : "transparent",
                  color: active ? ink : "#FFFFFF",
                  fontFamily: displayFont,
                  fontWeight: 600,
                  fontSize: "0.95rem",
                  opacity: active ? 1 : 0.5,
                  "@media (prefers-reduced-motion: no-preference)": active ? {} : { animation: `${breathe} 5.5s ease-in-out ${wave}ms infinite` },
                }}
              >
                {module.code}
              </Box>
            );
          })}
        </Box>
      </Box>

      <Typography sx={{ color: "rgba(255, 255, 255, 0.65)" }}>
        {availableModules.length} de {allModules.length} módulos em operação.
      </Typography>
    </Box>
  );
}
