import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Typography from "@mui/material/Typography";
import { inkSoft } from "@/lib/tokens";
import type { WorkflowStats as Stats } from "../types";

const ITEMS = [
  { key: "total", label: "Total" },
  { key: "inProgress", label: "Em andamento" },
  { key: "approved", label: "Aprovados" },
  { key: "rejected", label: "Rejeitados" },
] as const;

export default function WorkflowStats({ stats }: { stats: Stats }) {
  return (
    <Box sx={{ display: "grid", gap: 2, gridTemplateColumns: { xs: "repeat(2, minmax(0, 1fr))", md: "repeat(4, minmax(0, 1fr))" } }}>
      {ITEMS.map((item) => (
        <Card key={item.key} sx={{ p: 2.5 }}>
          <Typography variant="overline" component="p" sx={{ color: inkSoft }}>
            {item.label}
          </Typography>
          <Typography component="p" sx={{ fontWeight: 600, fontSize: "2rem", lineHeight: 1.1, mt: 0.5, fontVariantNumeric: "tabular-nums" }}>
            {stats[item.key]}
          </Typography>
        </Card>
      ))}
    </Box>
  );
}
