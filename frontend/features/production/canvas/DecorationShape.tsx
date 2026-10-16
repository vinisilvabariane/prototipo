import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { inkSoft, line, lineStrong, surface } from "@/lib/tokens";
import type { DecorationKind } from "../types";

type Props = { kind: DecorationKind; label: string | null; width: number; height: number };

const WARNING = "var(--mui-palette-warning-main)";
const STRIPE = `repeating-linear-gradient(135deg, ${WARNING} 0 12px, transparent 12px 24px)`;

function Label({ text, dark = false }: { text: string | null; dark?: boolean }) {
  if (!text) return null;

  return (
    <Typography
      noWrap
      sx={{ position: "absolute", inset: 0, display: "grid", placeItems: "center", px: 1, fontSize: 12, fontWeight: 600, letterSpacing: "0.04em", textTransform: "uppercase", color: dark ? "var(--mui-palette-warning-contrastText)" : inkSoft, pointerEvents: "none" }}
    >
      {text}
    </Typography>
  );
}

export default function DecorationShape({ kind, label, width, height }: Props) {
  if (kind === "belt") {
    return (
      <Box sx={{ position: "relative", width: "100%", height: "100%", borderRadius: 999, border: `2px solid ${lineStrong}`, bgcolor: surface, overflow: "hidden" }}>
        <Box
          sx={{
            position: "absolute",
            inset: 0,
            backgroundImage: `repeating-linear-gradient(90deg, ${line} 0 26px, transparent 26px 52px)`,
            "@media (prefers-reduced-motion: no-preference)": { animation: "belt-run 1.6s linear infinite" },
            "@keyframes belt-run": { from: { backgroundPositionX: 0 }, to: { backgroundPositionX: "52px" } },
          }}
        />
        <Label text={label} />
      </Box>
    );
  }

  if (kind === "zone") {
    return (
      <Box sx={{ position: "relative", width: "100%", height: "100%", borderRadius: "14px", border: `2px dashed ${lineStrong}`, bgcolor: "color-mix(in srgb, var(--mui-palette-text-primary) 3%, transparent)" }}>
        {label && (
          <Typography noWrap sx={{ position: "absolute", top: 8, left: 12, right: 12, fontSize: 12, fontWeight: 600, letterSpacing: "0.06em", textTransform: "uppercase", color: inkSoft }}>
            {label}
          </Typography>
        )}
      </Box>
    );
  }

  if (kind === "arrow") {
    const head = Math.min(height, 44);
    return (
      <Box component="svg" viewBox={`0 0 ${width} ${height}`} preserveAspectRatio="none" sx={{ width: "100%", height: "100%", display: "block" }}>
        <path
          d={`M0 ${height * 0.35} H${width - head} V0 L${width} ${height / 2} L${width - head} ${height} V${height * 0.65} H0 Z`}
          fill="color-mix(in srgb, var(--mui-palette-primary-main) 18%, transparent)"
          stroke="var(--mui-palette-primary-main)"
          strokeWidth={2}
          vectorEffect="non-scaling-stroke"
        />
      </Box>
    );
  }

  if (kind === "machine") {
    return (
      <Box sx={{ position: "relative", width: "100%", height: "100%", borderRadius: "10px", border: `2px solid ${lineStrong}`, bgcolor: surface, "&::before": { content: '""', position: "absolute", inset: 8, borderRadius: "6px", border: `1px solid ${line}` } }}>
        <Label text={label} />
      </Box>
    );
  }

  if (kind === "caution") {
    return (
      <Box sx={{ position: "relative", width: "100%", height: "100%", borderRadius: "6px", border: `2px solid ${WARNING}`, backgroundImage: STRIPE, opacity: 0.9 }}>
        {label && (
          <Box sx={{ position: "absolute", inset: "auto 12px", top: "50%", transform: "translateY(-50%)", bgcolor: WARNING, borderRadius: "4px", py: 0.25 }}>
            <Typography noWrap sx={{ textAlign: "center", fontSize: 12, fontWeight: 700, letterSpacing: "0.06em", textTransform: "uppercase", color: "var(--mui-palette-warning-contrastText)" }}>
              {label}
            </Typography>
          </Box>
        )}
      </Box>
    );
  }

  if (kind === "rack") {
    const shelves = Math.max(2, Math.floor(height / 36));
    return (
      <Box sx={{ position: "relative", width: "100%", height: "100%", borderRadius: "6px", border: `2px solid ${lineStrong}`, bgcolor: surface }}>
        {Array.from({ length: shelves }).map((_, index) => (
          <Box key={index} sx={{ position: "absolute", left: 6, right: 6, top: `${((index + 1) / (shelves + 1)) * 100}%`, height: 3, borderRadius: 2, bgcolor: lineStrong }} />
        ))}
        <Label text={label} />
      </Box>
    );
  }

  if (kind === "table") {
    return (
      <Box sx={{ position: "relative", width: "100%", height: "100%", borderRadius: "8px", border: `2px solid ${lineStrong}`, bgcolor: surface, "&::after": { content: '""', position: "absolute", inset: 8, borderRadius: "4px", border: `1px dashed ${line}` } }}>
        <Label text={label} />
      </Box>
    );
  }

  return (
    <Box sx={{ position: "relative", width: "100%", height: "100%", border: `2px solid ${lineStrong}`, bgcolor: "color-mix(in srgb, var(--mui-palette-text-primary) 12%, transparent)" }}>
      <Box component="svg" viewBox="0 0 10 10" preserveAspectRatio="none" sx={{ position: "absolute", inset: 0, width: "100%", height: "100%" }}>
        <path d="M0 0 L10 10 M10 0 L0 10" stroke={lineStrong} strokeWidth={1} vectorEffect="non-scaling-stroke" />
      </Box>
    </Box>
  );
}
