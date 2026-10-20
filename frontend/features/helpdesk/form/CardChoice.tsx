"use client";

import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import Typography from "@mui/material/Typography";
import { brand, brandSoft, inkSoft, line } from "@/lib/tokens";
import { HelpdeskIcon } from "../icons";
import type { FormOption } from "../types";

type Props = { label: string; options: FormOption[]; value: string; onChange: (value: string) => void; disabled?: boolean };

export default function CardChoice({ label, options, value, onChange, disabled }: Props) {
  return (
    <Box
      role="radiogroup"
      aria-label={label}
      sx={{ display: "grid", gap: 1.25, gridTemplateColumns: { xs: "repeat(2, minmax(0, 1fr))", md: "repeat(auto-fill, minmax(170px, 1fr))" } }}
    >
      {options.map((option) => {
        const selected = option.value === value;
        const accent = option.color || brand;

        return (
          <ButtonBase
            key={option.value}
            role="radio"
            aria-checked={selected}
            disabled={disabled}
            onClick={() => onChange(selected ? "" : option.value)}
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 1.25,
              textAlign: "left",
              px: 1.5,
              py: 1.25,
              borderRadius: 1.5,
              border: `1.5px solid ${selected ? accent : line}`,
              bgcolor: selected ? brandSoft : "background.paper",
              boxShadow: selected ? `inset 0 0 0 1px ${accent}` : "none",
              transition: "border-color 120ms, background-color 120ms",
              "&:hover:not(:disabled)": { borderColor: accent },
            }}
          >
            {option.icon && (
              <Box sx={{ width: 32, height: 32, borderRadius: 1, display: "grid", placeItems: "center", flexShrink: 0, color: selected ? "#fff" : accent, bgcolor: selected ? accent : "transparent", border: `1px solid ${selected ? accent : line}` }}>
                <HelpdeskIcon name={option.icon} fontSize="small" />
              </Box>
            )}
            <Typography variant="body2" sx={{ fontWeight: selected ? 700 : 600, color: selected ? "text.primary" : inkSoft, lineHeight: 1.3 }}>
              {option.label}
            </Typography>
          </ButtonBase>
        );
      })}
    </Box>
  );
}
