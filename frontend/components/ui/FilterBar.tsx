"use client";

import { useEffect, useRef, useState, useTransition, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import Close from "@mui/icons-material/Close";
import Search from "@mui/icons-material/Search";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import LinearProgress from "@mui/material/LinearProgress";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";

export type FilterField =
  | { type: "search"; name: string; label: string; width?: number }
  | { type: "select"; name: string; label: string; options: { value: string; label: string }[]; width?: number }
  | { type: "segment"; name: string; label: string; options: { value: string; label: string }[] }
  | { type: "date"; name: string; label: string; width?: number };

type Props = {
  basePath: string;
  fields: FilterField[];
  values: Record<string, string>;
  trailing?: ReactNode;
};

const SEARCH_DELAY_MS = 350;

export default function FilterBar({ basePath, fields, values, trailing }: Props) {
  const router = useRouter();
  const [pending, startTransition] = useTransition();
  const [draft, setDraft] = useState<Record<string, string>>(values);
  const timer = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  useEffect(() => () => clearTimeout(timer.current), []);

  function navigate(next: Record<string, string>) {
    const query = new URLSearchParams();
    for (const [key, value] of Object.entries(next)) if (value) query.set(key, value);
    const text = query.toString();
    startTransition(() => router.replace(text ? `${basePath}?${text}` : basePath));
  }

  function change(field: FilterField, value: string) {
    const next = { ...draft, [field.name]: value };
    setDraft(next);
    clearTimeout(timer.current);

    if (field.type === "search") timer.current = setTimeout(() => navigate(next), SEARCH_DELAY_MS);
    else navigate(next);
  }

  function clear() {
    clearTimeout(timer.current);
    const empty = Object.fromEntries(fields.map((field) => [field.name, field.type === "segment" ? (draft[field.name] ?? "") : ""]));
    setDraft(empty);
    navigate(empty);
  }

  const active = fields.some((field) => field.type !== "segment" && draft[field.name]);

  return (
    <Box sx={{ position: "relative" }}>
      <Stack direction="row" spacing={1.5} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1.5 }}>
        {fields.map((field) =>
          field.type === "segment" ? (
            <ToggleButtonGroup
              key={field.name}
              exclusive
              size="small"
              aria-label={field.label}
              value={draft[field.name] ?? ""}
              onChange={(_event, value: string | null) => value !== null && change(field, value)}
            >
              {field.options.map((option) => (
                <ToggleButton key={option.value} value={option.value} sx={{ px: 1.75, textTransform: "none", fontWeight: 600 }}>
                  {option.label}
                </ToggleButton>
              ))}
            </ToggleButtonGroup>
          ) : field.type === "date" ? (
            <TextField
              key={field.name}
              size="small"
              type="date"
              label={field.label}
              value={draft[field.name] ?? ""}
              onChange={(event) => change(field, event.target.value)}
              sx={{ width: field.width ?? 160 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          ) : field.type === "search" ? (
            <TextField
              key={field.name}
              size="small"
              placeholder={field.label}
              value={draft[field.name] ?? ""}
              onChange={(event) => change(field, event.target.value)}
              sx={{ width: { xs: "100%", sm: field.width ?? 320 } }}
              slotProps={{
                htmlInput: { "aria-label": field.label },
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <Search fontSize="small" />
                    </InputAdornment>
                  ),
                  endAdornment: draft[field.name] ? (
                    <InputAdornment position="end">
                      <IconButton size="small" aria-label="Limpar busca" onClick={() => change(field, "")}>
                        <Close fontSize="small" />
                      </IconButton>
                    </InputAdornment>
                  ) : undefined,
                },
              }}
            />
          ) : (
            <TextField
              key={field.name}
              select
              size="small"
              label={field.label}
              value={draft[field.name] ?? ""}
              onChange={(event) => change(field, event.target.value)}
              sx={{ minWidth: field.width ?? 170 }}
              slotProps={{ inputLabel: { shrink: true }, select: { displayEmpty: true, renderValue: (value) => field.options.find((option) => option.value === value)?.label ?? "Todos" } }}
            >
              <MenuItem value="">Todos</MenuItem>
              {field.options.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>
          ),
        )}
        {active && (
          <Button size="small" onClick={clear}>
            Limpar filtros
          </Button>
        )}
        {trailing && <Box sx={{ ml: "auto" }}>{trailing}</Box>}
      </Stack>
      {pending && <LinearProgress sx={{ position: "absolute", left: -16, right: -16, bottom: -12, height: 2 }} />}
    </Box>
  );
}
