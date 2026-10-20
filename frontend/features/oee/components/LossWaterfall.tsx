import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { brand, inkSoft, line, lineStrong } from "@/lib/tokens";
import { formatOee } from "../format";
import type { OeePillars } from "../types";

const clamp = (value: number) => Math.min(1, Math.max(0, value / 100));

const PATTERNS = {
  availability: `repeating-linear-gradient(135deg, ${lineStrong} 0 3px, transparent 3px 7px)`,
  performance: lineStrong,
  quality: `repeating-linear-gradient(90deg, ${lineStrong} 0 2px, transparent 2px 5px)`,
} as const;

function Legend({ swatch, label, value }: { swatch: ReactNode; label: string; value: number }) {
  return (
    <Stack direction="row" spacing={0.75} sx={{ alignItems: "center" }}>
      {swatch}
      <Typography variant="body2" sx={{ color: inkSoft }}>
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 700, fontVariantNumeric: "tabular-nums" }}>
        {formatOee(value)}
      </Typography>
    </Stack>
  );
}

export default function LossWaterfall({ pillars }: { pillars: OeePillars }) {
  const a = clamp(pillars.availability);
  const p = clamp(pillars.performance);
  const q = clamp(pillars.quality);

  const afterA = a * 100;
  const afterP = afterA * p;
  const kept = afterP * q;

  const steps = [
    { key: "availability", label: "Perda por paradas", value: 100 - afterA },
    { key: "performance", label: "Perda de ritmo", value: afterA - afterP },
    { key: "quality", label: "Perda por defeitos", value: afterP - kept },
  ] as const;

  return (
    <Box>
      <Box
        role="img"
        aria-label={`De 100% do tempo teórico, ${formatOee(kept)} viram produção boa. ${steps.map((step) => `${step.label}: ${formatOee(step.value)}`).join(". ")}.`}
        sx={{ display: "flex", height: 30, borderRadius: 1, overflow: "hidden", bgcolor: line, border: `1px solid ${lineStrong}` }}
      >
        <Box sx={{ width: `${kept}%`, bgcolor: brand }} />
        {steps.map((step) => (
          <Box key={step.key} sx={{ width: `${step.value}%`, background: PATTERNS[step.key], borderLeft: step.value > 0 ? `1px solid ${lineStrong}` : 0 }} />
        ))}
      </Box>
      <Stack direction="row" sx={{ mt: 1, flexWrap: "wrap", columnGap: 2.5, rowGap: 0.75 }}>
        <Legend swatch={<Box sx={{ width: 12, height: 12, borderRadius: "3px", bgcolor: brand }} />} label="Produção boa" value={kept} />
        {steps.map((step) => (
          <Legend
            key={step.key}
            swatch={<Box sx={{ width: 12, height: 12, borderRadius: "3px", border: `1px solid ${lineStrong}`, background: PATTERNS[step.key] }} />}
            label={step.label}
            value={step.value}
          />
        ))}
      </Stack>
    </Box>
  );
}
