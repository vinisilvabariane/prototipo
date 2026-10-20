import Box from "@mui/material/Box";
import { ink, inkSoft, line, lineStrong } from "@/lib/tokens";
import { bandColor, formatHour, formatOee, formatQuantity } from "../format";
import type { OeeHour } from "../types";

const WIDTH = 720;
const HEIGHT = 190;
const PAD = { top: 12, right: 8, bottom: 26, left: 34 };

export default function HourColumns({ hours }: { hours: OeeHour[] }) {
  const plotWidth = WIDTH - PAD.left - PAD.right;
  const plotHeight = HEIGHT - PAD.top - PAD.bottom;
  const max = Math.max(100, ...hours.map((hour) => hour.pillars.oee));
  const slot = plotWidth / Math.max(hours.length, 1);
  const barWidth = Math.min(44, slot * 0.62);
  const y = (value: number) => PAD.top + plotHeight - (value / max) * plotHeight;

  return (
    <Box
      component="svg"
      viewBox={`0 0 ${WIDTH} ${HEIGHT}`}
      role="img"
      aria-label={`OEE da linha hora a hora: ${hours.map((hour) => `${formatHour(hour.hourStart)} ${formatOee(hour.pillars.oee)}`).join(", ")}`}
      sx={{ width: "100%", height: "auto", display: "block", maxHeight: 240 }}
    >
      {[0, 50, 100].map((tick) => (
        <g key={tick}>
          <line x1={PAD.left} x2={WIDTH - PAD.right} y1={y(tick)} y2={y(tick)} stroke={tick === 0 ? lineStrong : line} strokeDasharray={tick === 0 ? undefined : "3 4"} />
          <text x={PAD.left - 6} y={y(tick) + 4} textAnchor="end" fontSize="11" fill={inkSoft}>
            {tick}%
          </text>
        </g>
      ))}
      {hours.map((hour, index) => {
        const x = PAD.left + slot * index + (slot - barWidth) / 2;
        const value = hour.pillars.oee;
        const partial = hour.seconds < 3600;
        const top = y(value);
        return (
          <g key={hour.hourStart}>
            <title>{`${formatHour(hour.hourStart)}${partial ? ` (parcial, ${Math.round(hour.seconds / 60)} min)` : ""} · OEE ${formatOee(value)} · ${formatQuantity(hour.produced)} de ${formatQuantity(hour.theoretical)} peças`}</title>
            <rect x={x} y={top} width={barWidth} height={Math.max(PAD.top + plotHeight - top, value > 0 ? 2 : 0)} rx="3" fill={bandColor(value)} fillOpacity={partial ? 0.45 : 1} stroke={partial ? bandColor(value) : undefined} strokeDasharray={partial ? "3 2" : undefined} />
            <text x={x + barWidth / 2} y={Math.max(top - 4, 10)} textAnchor="middle" fontSize="11" fontWeight="600" fill={ink}>
              {Math.round(value)}
            </text>
            <text x={x + barWidth / 2} y={HEIGHT - 8} textAnchor="middle" fontSize="11" fill={inkSoft}>
              {formatHour(hour.hourStart)}
            </text>
          </g>
        );
      })}
    </Box>
  );
}
