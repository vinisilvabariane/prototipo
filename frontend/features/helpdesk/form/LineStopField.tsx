"use client";

import Box from "@mui/material/Box";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { inkSoft } from "@/lib/tokens";
import { TIME_SUFFIX } from "../schema";
import type { Answers, FormField } from "../types";

type Props = { field: FormField; answers: Answers; onChange: (patch: Answers) => void; disabled?: boolean };

const pad = (value: number) => String(value).padStart(2, "0");

function toLocalInput(iso: string | undefined) {
  if (!iso) return "";
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? "" : `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function fromLocalInput(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "" : date.toISOString();
}

export default function LineStopField({ field, answers, onChange, disabled }: Props) {
  const state = answers[field.id] ?? "";
  const time = answers[field.id + TIME_SUFFIX];

  function choose(next: string | null) {
    if (next === null) return;
    const now = new Date();
    now.setSeconds(0, 0);
    onChange({ [field.id]: next, [field.id + TIME_SUFFIX]: next === "stopped" ? now.toISOString() : "" });
  }

  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 1.5 }}>
      <ToggleButtonGroup exclusive value={state || null} onChange={(_event, next: string | null) => choose(next)} disabled={disabled} aria-label={field.label} sx={{ alignSelf: "flex-start" }}>
        <ToggleButton value="running" sx={{ px: 2.5, textTransform: "none", fontWeight: 600, "&.Mui-selected": { bgcolor: "success.main", color: "success.contrastText", "&:hover": { bgcolor: "success.main" } } }}>
          Rodando
        </ToggleButton>
        <ToggleButton value="stopped" sx={{ px: 2.5, textTransform: "none", fontWeight: 600, "&.Mui-selected": { bgcolor: "error.main", color: "error.contrastText", "&:hover": { bgcolor: "error.main" } } }}>
          Parada
        </ToggleButton>
      </ToggleButtonGroup>
      {state === "stopped" && (
        <Box>
          <TextField
            type="datetime-local"
            label="Parada desde"
            value={toLocalInput(time)}
            onChange={(event) => onChange({ [field.id + TIME_SUFFIX]: fromLocalInput(event.target.value) })}
            slotProps={{ inputLabel: { shrink: true } }}
            sx={{ maxWidth: 260 }}
            disabled={disabled}
          />
          <Typography variant="caption" sx={{ color: inkSoft, display: "block", mt: 0.5 }}>
            O tempo parado conta como perda de disponibilidade no OEE enquanto o chamado não for resolvido.
          </Typography>
        </Box>
      )}
    </Box>
  );
}
