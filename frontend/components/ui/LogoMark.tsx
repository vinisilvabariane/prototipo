import Box from "@mui/material/Box";
import { brand } from "@/lib/tokens";

type Props = { size?: number; tone?: "light" | "dark" };

export default function LogoMark({ size = 22, tone = "light" }: Props) {
  const filled = tone === "light" ? brand : "#FFFFFF";
  const outline = tone === "light" ? "rgba(10, 19, 48, 0.35)" : "rgba(255, 255, 255, 0.45)";
  const gap = Math.max(2, Math.round(size / 9));
  const radius = Math.round(size / 6);

  return (
    <Box
      aria-hidden
      sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: `${gap}px`, width: size, height: size, flexShrink: 0 }}
    >
      <Box sx={{ bgcolor: filled, borderRadius: `${radius}px` }} />
      {[0, 1, 2].map((cell) => (
        <Box key={cell} sx={{ border: `1.5px solid ${outline}`, borderRadius: `${radius}px` }} />
      ))}
    </Box>
  );
}
