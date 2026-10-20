"use client";

import AddOutlined from "@mui/icons-material/AddOutlined";
import ArrowDownward from "@mui/icons-material/ArrowDownward";
import ArrowUpward from "@mui/icons-material/ArrowUpward";
import DeleteOutlined from "@mui/icons-material/DeleteOutlined";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import { inkSoft, line } from "@/lib/tokens";
import { HelpdeskIcon, ICON_KEYS, ICON_LABELS } from "../icons";
import { newOption } from "../schema";
import type { FormField, FormOption } from "../types";

type Props = {
  field: FormField;
  childCount: (value: string) => number;
  onChange: (options: FormOption[]) => void;
  onRemove: (value: string) => void;
};

export default function OptionsEditor({ field, childCount, onChange, onRemove }: Props) {
  const withIcons = field.type === "cardRadio";

  function patch(index: number, change: Partial<FormOption>) {
    onChange(field.options.map((option, i) => (i === index ? { ...option, ...change } : option)));
  }

  function move(index: number, direction: -1 | 1) {
    const target = index + direction;
    if (target < 0 || target >= field.options.length) return;
    const copy = [...field.options];
    [copy[index], copy[target]] = [copy[target], copy[index]];
    onChange(copy);
  }

  return (
    <Stack spacing={1.25}>
      <Typography variant="overline" component="h4" sx={{ color: inkSoft }}>
        Opções · {field.options.length}
      </Typography>
      {field.options.map((option, index) => {
        const children = childCount(option.value);

        return (
          <Box key={option.value} sx={{ border: `1px solid ${line}`, borderRadius: 1, p: 1 }}>
            <Stack direction="row" spacing={0.5} sx={{ alignItems: "center" }}>
              <TextField size="small" value={option.label} onChange={(event) => patch(index, { label: event.target.value })} placeholder="Texto da opção" fullWidth slotProps={{ htmlInput: { "aria-label": `Opção ${index + 1}`, maxLength: 100 } }} />
              <Tooltip title="Subir">
                <span>
                  <IconButton size="small" onClick={() => move(index, -1)} disabled={index === 0} aria-label="Subir opção">
                    <ArrowUpward fontSize="small" />
                  </IconButton>
                </span>
              </Tooltip>
              <Tooltip title="Descer">
                <span>
                  <IconButton size="small" onClick={() => move(index, 1)} disabled={index === field.options.length - 1} aria-label="Descer opção">
                    <ArrowDownward fontSize="small" />
                  </IconButton>
                </span>
              </Tooltip>
              <Tooltip title={children > 0 ? `Remove a opção e as ${children} pergunta(s) que dependem dela` : "Remover opção"}>
                <span>
                  <IconButton size="small" color="error" onClick={() => onRemove(option.value)} disabled={field.options.length <= 1} aria-label="Remover opção">
                    <DeleteOutlined fontSize="small" />
                  </IconButton>
                </span>
              </Tooltip>
            </Stack>
            {withIcons && (
              <Stack direction="row" spacing={1} sx={{ mt: 1, alignItems: "center" }}>
                <TextField select size="small" label="Ícone" value={option.icon ?? ""} onChange={(event) => patch(index, { icon: event.target.value || null })} sx={{ minWidth: 150 }}>
                  <MenuItem value="">Sem ícone</MenuItem>
                  {ICON_KEYS.map((key) => (
                    <MenuItem key={key} value={key}>
                      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                        <HelpdeskIcon name={key} fontSize="small" />
                        <span>{ICON_LABELS[key]}</span>
                      </Stack>
                    </MenuItem>
                  ))}
                </TextField>
                <Tooltip title="Cor de destaque do card">
                  <Box component="input" type="color" aria-label="Cor da opção" value={option.color || "#2547D0"} onChange={(event) => patch(index, { color: event.target.value })} sx={{ width: 36, height: 36, p: 0, border: `1px solid ${line}`, borderRadius: 1, bgcolor: "transparent", cursor: "pointer" }} />
                </Tooltip>
              </Stack>
            )}
          </Box>
        );
      })}
      <Button size="small" startIcon={<AddOutlined />} onClick={() => onChange([...field.options, newOption(field)])} sx={{ alignSelf: "flex-start" }}>
        Adicionar opção
      </Button>
    </Stack>
  );
}
