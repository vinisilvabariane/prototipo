"use client";

import Box from "@mui/material/Box";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { brand, inkSoft, line } from "@/lib/tokens";
import { visibleFields } from "../schema";
import type { Answers, FormField, Location } from "../types";
import CardChoice from "./CardChoice";
import LineStopField from "./LineStopField";
import SpotField from "./SpotField";

type Props = {
  fields: FormField[];
  answers: Answers;
  onChange: (answers: Answers) => void;
  locations?: Location[];
  disabled?: boolean;
};

function Control({ field, answers, patch, locations, disabled }: { field: FormField; answers: Answers; patch: (value: Answers) => void; locations: Location[]; disabled?: boolean }) {
  const value = answers[field.id] ?? "";
  const set = (next: string) => patch({ [field.id]: next });

  switch (field.type) {
    case "text":
      return <TextField value={value} onChange={(event) => set(event.target.value)} placeholder={field.placeholder ?? undefined} disabled={disabled} fullWidth hiddenLabel />;
    case "textArea":
      return <TextField value={value} onChange={(event) => set(event.target.value)} placeholder={field.placeholder ?? undefined} disabled={disabled} multiline minRows={3} fullWidth hiddenLabel />;
    case "number":
      return (
        <TextField
          value={value}
          onChange={(event) => set(event.target.value)}
          placeholder={field.placeholder ?? undefined}
          disabled={disabled}
          slotProps={{ htmlInput: { inputMode: "decimal" } }}
          sx={{ maxWidth: 240 }}
          hiddenLabel
        />
      );
    case "defects":
      return (
        <TextField
          value={value}
          onChange={(event) => set(event.target.value.replace(/\D/g, ""))}
          placeholder="0"
          disabled={disabled}
          slotProps={{ htmlInput: { inputMode: "numeric" } }}
          sx={{ maxWidth: 160 }}
          hiddenLabel
        />
      );
    case "select":
      return (
        <TextField select value={value} onChange={(event) => set(event.target.value)} disabled={disabled} fullWidth hiddenLabel slotProps={{ select: { displayEmpty: true, renderValue: (selected) => field.options.find((option) => option.value === selected)?.label ?? "Selecione…" } }}>
          {field.options.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      );
    case "cardRadio":
      return <CardChoice label={field.label} options={field.options} value={value} onChange={set} disabled={disabled} />;
    case "productionSpot":
      return <SpotField field={field} answers={answers} locations={locations} onChange={patch} disabled={disabled} />;
    case "lineStop":
      return <LineStopField field={field} answers={answers} onChange={patch} disabled={disabled} />;
  }
}

export default function FormRenderer({ fields, answers, onChange, locations = [], disabled }: Props) {
  const shown = visibleFields(fields, answers);

  function patch(next: Answers) {
    onChange({ ...answers, ...next });
  }

  return (
    <Stack spacing={2.5}>
      {shown.map((field) => (
        <Box key={field.id} sx={{ pl: field.dependsOn ? 2 : 0, borderLeft: field.dependsOn ? `2px solid ${line}` : 0 }}>
          <Typography component="label" variant="body2" sx={{ fontWeight: 700, display: "block", mb: 0.75 }}>
            {field.label}
            {field.required && (
              <Box component="span" sx={{ color: brand, ml: 0.5 }} aria-hidden>
                *
              </Box>
            )}
            {!field.required && (
              <Typography component="span" variant="caption" sx={{ color: inkSoft, ml: 1, fontWeight: 500 }}>
                opcional
              </Typography>
            )}
          </Typography>
          <Control field={field} answers={answers} patch={patch} locations={locations} disabled={disabled} />
        </Box>
      ))}
      {shown.length === 0 && (
        <Typography variant="body2" sx={{ color: inkSoft }}>
          Este formulário ainda não tem perguntas.
        </Typography>
      )}
    </Stack>
  );
}
