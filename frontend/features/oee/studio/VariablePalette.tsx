"use client";

import { useState } from "react";
import Search from "@mui/icons-material/Search";
import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import InputAdornment from "@mui/material/InputAdornment";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { brandSoft, inkSoft, line } from "@/lib/tokens";
import type { OeeVariable } from "../types";

type Props = { variables: OeeVariable[]; disabled?: boolean; onPick: (key: string) => void };

export default function VariablePalette({ variables, disabled, onPick }: Props) {
  const [search, setSearch] = useState("");
  const term = search.trim().toLowerCase();
  const shown = term ? variables.filter((variable) => `${variable.label} ${variable.key} ${variable.description}`.toLowerCase().includes(term)) : variables;

  return (
    <Box sx={{ border: `1px solid ${line}`, borderRadius: 1, overflow: "hidden" }}>
      <Stack direction="row" spacing={1.5} sx={{ alignItems: "center", px: 1.5, py: 1, borderBottom: `1px solid ${line}` }}>
        <Typography variant="overline" component="h3" sx={{ color: inkSoft, whiteSpace: "nowrap" }}>
          Variáveis · {shown.length}
        </Typography>
        <TextField
          size="small"
          placeholder="Buscar variável"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          fullWidth
          slotProps={{
            htmlInput: { "aria-label": "Buscar variável" },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search fontSize="small" />
                </InputAdornment>
              ),
            },
          }}
        />
      </Stack>
      <Box component="ul" sx={{ listStyle: "none", m: 0, p: 0, maxHeight: 280, overflowY: "auto" }}>
        {shown.length === 0 && (
          <Typography component="li" variant="body2" sx={{ color: inkSoft, p: 2 }}>
            Nenhuma variável com esse termo.
          </Typography>
        )}
        {shown.map((variable) => (
          <li key={variable.key}>
            <ButtonBase
              disabled={disabled}
              onClick={() => onPick(variable.key)}
              sx={{ display: "block", width: "100%", textAlign: "left", px: 1.5, py: 0.875, borderTop: `1px solid ${line}`, "&:first-of-type": { borderTop: 0 }, "&:hover:not(:disabled)": { bgcolor: brandSoft } }}
            >
              <Stack direction="row" spacing={1} sx={{ alignItems: "baseline", flexWrap: "wrap" }}>
                <Typography component="span" variant="body2" sx={{ fontWeight: 700 }}>
                  {variable.label}
                </Typography>
                <Typography component="span" variant="caption" sx={{ color: inkSoft, fontWeight: 600 }}>
                  {variable.key}
                  {variable.unit ? ` · ${variable.unit}` : ""}
                </Typography>
              </Stack>
              <Typography component="span" variant="caption" sx={{ color: inkSoft, display: "block" }}>
                {variable.description}
              </Typography>
            </ButtonBase>
          </li>
        ))}
      </Box>
    </Box>
  );
}
