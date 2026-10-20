import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import Field from "@/components/ui/Field";
import Panel from "@/components/ui/Panel";
import { inkSoft, line } from "@/lib/tokens";

export function Section({ title, count, action, children }: { title: string; count?: number; action?: ReactNode; children: ReactNode }) {
  return (
    <Panel title={title} count={count} action={action}>
      {children}
    </Panel>
  );
}

export function Empty({ children }: { children: ReactNode }) {
  return <Typography sx={{ color: inkSoft }}>{children}</Typography>;
}

export function ItemBox({ title, meta, actions, children }: { title: ReactNode; meta?: ReactNode; actions?: ReactNode; children?: ReactNode }) {
  return (
    <Box sx={{ border: `1px solid ${line}`, borderRadius: "12px", p: 1.5 }}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1} sx={{ justifyContent: "space-between", alignItems: { sm: "flex-start" } }}>
        <Box sx={{ minWidth: 0 }}>
          <Typography component="div" sx={{ fontWeight: 600, overflowWrap: "anywhere" }}>
            {title}
          </Typography>
          {meta && (
            <Typography component="div" variant="body2" sx={{ color: inkSoft, mt: 0.25, fontVariantNumeric: "tabular-nums" }}>
              {meta}
            </Typography>
          )}
        </Box>
        {actions && <Stack direction="row" spacing={0.5} sx={{ flexShrink: 0, alignItems: "center" }}>{actions}</Stack>}
      </Stack>
      {children}
    </Box>
  );
}

export function ItemList({ children }: { children: ReactNode }) {
  return <Box sx={{ display: "grid", gap: 1 }}>{children}</Box>;
}

export { Field };
