import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { line } from "@/lib/tokens";
import { bandColor, formatOee } from "../format";

type Props = { value: number; width?: number; strong?: boolean; tone?: "band" | "neutral" };

export default function OeeBar({ value, width = 56, strong, tone = "band" }: Props) {
  return (
    <Box sx={{ display: "inline-flex", alignItems: "center", gap: 1, justifyContent: "flex-end" }}>
      <Box sx={{ width, height: 6, borderRadius: 3, bgcolor: line, overflow: "hidden", display: { xs: "none", sm: "block" } }}>
        <Box sx={{ height: "100%", width: `${Math.min(100, Math.max(0, value))}%`, bgcolor: tone === "band" ? bandColor(value) : "text.primary", opacity: tone === "band" ? 1 : 0.55 }} />
      </Box>
      <Typography component="span" variant="body2" sx={{ minWidth: 48, textAlign: "right", fontWeight: strong ? 700 : 500, fontVariantNumeric: "tabular-nums" }}>
        {formatOee(value)}
      </Typography>
    </Box>
  );
}
