import type { ReactNode } from "react";
import ArrowBack from "@mui/icons-material/ArrowBack";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft } from "@/lib/tokens";

type Props = {
  title: string;
  eyebrow?: ReactNode;
  description?: ReactNode;
  badges?: ReactNode;
  back?: { href: string; label: string };
  actions?: ReactNode;
};

export default function PageHeader({ title, eyebrow, description, badges, back, actions }: Props) {
  return (
    <Box>
      {back && (
        <Button href={back.href} size="small" startIcon={<ArrowBack fontSize="small" />} sx={{ color: inkSoft, ml: -1, mb: 0.5 }}>
          {back.label}
        </Button>
      )}
      <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ justifyContent: "space-between", alignItems: { md: "center" } }}>
        <Box sx={{ minWidth: 0 }}>
          {eyebrow && (
            <Typography variant="overline" component="p" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
              {eyebrow}
            </Typography>
          )}
          <Stack direction="row" spacing={1.5} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 0.5 }}>
            <Typography variant="h4" component="h1" sx={{ overflowWrap: "anywhere" }}>
              {title}
            </Typography>
            {badges}
          </Stack>
          {description && (
            <Typography variant="body2" component="div" sx={{ color: inkSoft, mt: 0.5, maxWidth: 720 }}>
              {description}
            </Typography>
          )}
        </Box>
        {actions && <Stack direction="row" spacing={1} sx={{ flexShrink: 0, flexWrap: "wrap", rowGap: 1, alignItems: "center" }}>{actions}</Stack>}
      </Stack>
    </Box>
  );
}
