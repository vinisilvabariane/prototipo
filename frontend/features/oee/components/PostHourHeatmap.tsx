import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { inkSoft, line, lineStrong } from "@/lib/tokens";
import { bandColor, formatHour, formatOee, formatQuantity } from "../format";
import type { OeePost } from "../types";

export default function PostHourHeatmap({ posts }: { posts: OeePost[] }) {
  const measured = posts.filter((post) => post.measured && post.hours.length > 0).sort((a, b) => a.x - b.x);
  if (measured.length === 0) return null;

  const hours = measured[0].hours;

  return (
    <Box sx={{ overflowX: "auto" }}>
      <Box
        role="table"
        aria-label="OEE de cada posto em cada hora"
        sx={{ display: "grid", gridTemplateColumns: `minmax(120px, 1.2fr) repeat(${hours.length}, minmax(52px, 1fr))`, gap: "3px", minWidth: 120 + hours.length * 56 }}
      >
        <Box role="row" sx={{ display: "contents" }}>
          <Box role="columnheader" />
          {hours.map((hour) => (
            <Typography key={hour.hourStart} role="columnheader" variant="caption" sx={{ color: inkSoft, textAlign: "center", fontVariantNumeric: "tabular-nums" }}>
              {formatHour(hour.hourStart)}
            </Typography>
          ))}
        </Box>
        {measured.map((post) => (
          <Box key={post.postId} role="row" sx={{ display: "contents" }}>
            <Typography role="rowheader" variant="body2" noWrap sx={{ alignSelf: "center", fontWeight: 600, pr: 1 }} title={post.name}>
              {post.name}
            </Typography>
            {post.hours.map((hour) => {
              const value = hour.pillars.oee;
              const partial = hour.seconds < 3600;
              return (
                <Box
                  key={hour.hourStart}
                  role="cell"
                  title={`${post.name} · ${formatHour(hour.hourStart)}${partial ? ` (parcial, ${Math.round(hour.seconds / 60)} min)` : ""} · OEE ${formatOee(value)} · ${formatQuantity(hour.produced)} de ${formatQuantity(hour.theoretical)} peças`}
                  sx={{
                    position: "relative",
                    height: 32,
                    borderRadius: "4px",
                    border: `1px ${partial ? "dashed" : "solid"} ${partial ? lineStrong : line}`,
                    display: "grid",
                    placeItems: "center",
                    overflow: "hidden",
                    "&::before": { content: '""', position: "absolute", inset: 0, bgcolor: bandColor(value), opacity: 0.2 },
                    "&::after": { content: '""', position: "absolute", left: 0, bottom: 0, height: 3, width: `${Math.min(100, value)}%`, bgcolor: bandColor(value) },
                  }}
                >
                  <Typography component="span" variant="caption" sx={{ position: "relative", fontWeight: 700, fontVariantNumeric: "tabular-nums" }}>
                    {Math.round(value)}
                  </Typography>
                </Box>
              );
            })}
          </Box>
        ))}
      </Box>
    </Box>
  );
}
