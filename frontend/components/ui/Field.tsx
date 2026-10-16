import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { inkSoft, line } from "@/lib/tokens";

export default function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "minmax(110px, 34%) minmax(0, 1fr)" },
        columnGap: 2,
        rowGap: 0.25,
        py: 1,
        borderBottom: `1px solid ${line}`,
        "&:last-of-type": { borderBottom: 0 },
      }}
    >
      <Typography variant="body2" sx={{ color: inkSoft, pt: "1px" }}>
        {label}
      </Typography>
      <Typography component="div" sx={{ fontWeight: 500, overflowWrap: "anywhere" }}>
        {children}
      </Typography>
    </Box>
  );
}
