import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import { line } from "@/lib/tokens";

type Props = { toolbar?: ReactNode; children: ReactNode; footer?: ReactNode };

export default function ListCard({ toolbar, children, footer }: Props) {
  return (
    <Card sx={{ overflow: "hidden" }}>
      {toolbar && <Box sx={{ px: 2, py: 1.5, borderBottom: `1px solid ${line}` }}>{toolbar}</Box>}
      {children}
      {footer}
    </Card>
  );
}
