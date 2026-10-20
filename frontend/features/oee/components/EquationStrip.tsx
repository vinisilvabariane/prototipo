import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { brandSoft, brandWash, inkSoft, line } from "@/lib/tokens";
import { bandColor, BAND_LABELS, bandOf, formatOee } from "../format";
import type { OeePillars } from "../types";

type Props = { pillars: OeePillars; compact?: boolean };

function Term({ label, value, compact }: { label: string; value: number; compact?: boolean }) {
  return (
    <Box sx={{ minWidth: 0 }}>
      <Typography variant="overline" component="p" sx={{ color: inkSoft, lineHeight: 1.4, whiteSpace: "nowrap" }}>
        {label}
      </Typography>
      <Typography component="p" sx={{ fontSize: compact ? 20 : 26, fontWeight: 700, lineHeight: 1.15, fontVariantNumeric: "tabular-nums" }}>
        {formatOee(value)}
      </Typography>
      <Box sx={{ height: 4, borderRadius: 2, bgcolor: line, mt: 0.75, overflow: "hidden" }}>
        <Box sx={{ height: "100%", width: `${Math.min(100, Math.max(0, value))}%`, bgcolor: "text.primary", opacity: 0.55 }} />
      </Box>
    </Box>
  );
}

const operator = (symbol: string) => (
  <Typography aria-hidden component="span" sx={{ display: { xs: "none", md: "block" }, color: inkSoft, fontSize: 22, fontWeight: 300, alignSelf: "center", pt: 1.5 }}>
    {symbol}
  </Typography>
);

export default function EquationStrip({ pillars, compact }: Props) {
  const band = bandOf(pillars.oee);

  return (
    <Box
      role="group"
      aria-label="Disponibilidade vezes desempenho vezes qualidade é igual ao OEE"
      sx={{
        display: "grid",
        gap: { xs: 1.5, md: compact ? 1.5 : 2.5 },
        gridTemplateColumns: { xs: "repeat(2, minmax(0, 1fr))", md: "minmax(0, 1fr) auto minmax(0, 1fr) auto minmax(0, 1fr) auto minmax(0, 1.35fr)" },
      }}
    >
      <Term label="Disponibilidade" value={pillars.availability} compact={compact} />
      {operator("×")}
      <Term label="Desempenho" value={pillars.performance} compact={compact} />
      {operator("×")}
      <Term label="Qualidade" value={pillars.quality} compact={compact} />
      {operator("=")}
      <Box sx={{ minWidth: 0, bgcolor: brandSoft, border: `1px solid ${brandWash}`, borderRadius: 1, px: 1.5, py: 0.75, gridColumn: { xs: "1 / -1", md: "auto" } }}>
        <Typography variant="overline" component="p" sx={{ color: inkSoft, lineHeight: 1.4 }}>
          OEE
        </Typography>
        <Typography component="p" sx={{ fontSize: compact ? 26 : 34, fontWeight: 800, lineHeight: 1.1, fontVariantNumeric: "tabular-nums" }}>
          {formatOee(pillars.oee)}
        </Typography>
        <Box sx={{ display: "flex", alignItems: "center", gap: 0.75, mt: 0.5 }}>
          <Box aria-hidden sx={{ width: 8, height: 8, borderRadius: "50%", bgcolor: bandColor(pillars.oee) }} />
          <Typography variant="caption" sx={{ color: inkSoft, fontWeight: 600 }}>
            {BAND_LABELS[band]}
          </Typography>
        </Box>
      </Box>
    </Box>
  );
}
