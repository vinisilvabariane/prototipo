import type { ReactNode } from "react";
import InboxOutlined from "@mui/icons-material/InboxOutlined";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { inkSoft, lineStrong } from "@/lib/tokens";

type Props = { title?: string; hint?: ReactNode; action?: ReactNode; compact?: boolean };

export default function EmptyState({ title = "Nada por aqui", hint, action, compact }: Props) {
  return (
    <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center", gap: 0.75, textAlign: "center", py: compact ? 3 : 5, px: 2 }}>
      <InboxOutlined sx={{ fontSize: 28, color: lineStrong }} />
      <Typography sx={{ fontWeight: 600 }}>{title}</Typography>
      {hint && (
        <Typography variant="body2" sx={{ color: inkSoft, maxWidth: 420 }}>
          {hint}
        </Typography>
      )}
      {action && <Box sx={{ mt: 1 }}>{action}</Box>}
    </Box>
  );
}
