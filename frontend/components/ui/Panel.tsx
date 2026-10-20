import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft, line } from "@/lib/tokens";

type Props = {
  title: string;
  count?: number;
  action?: ReactNode;
  flush?: boolean;
  children: ReactNode;
};

export default function Panel({ title, count, action, flush, children }: Props) {
  return (
    <Card sx={{ overflow: "hidden" }}>
      <Stack
        direction="row"
        spacing={2}
        sx={{ justifyContent: "space-between", alignItems: "center", minHeight: 46, px: 2.5, py: 0.75, borderBottom: flush ? `1px solid ${line}` : 0 }}
      >
        <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
          {title}
          {count !== undefined && ` · ${count}`}
        </Typography>
        {action && <Box sx={{ flexShrink: 0 }}>{action}</Box>}
      </Stack>
      <Box sx={flush ? undefined : { px: 2.5, pb: 2.5 }}>{children}</Box>
    </Card>
  );
}
