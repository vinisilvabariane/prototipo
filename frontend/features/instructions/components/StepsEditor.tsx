"use client";

import { useState } from "react";
import Add from "@mui/icons-material/Add";
import ArrowDownward from "@mui/icons-material/ArrowDownward";
import ArrowUpward from "@mui/icons-material/ArrowUpward";
import Close from "@mui/icons-material/Close";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { formatBytes } from "@/lib/format";
import { brand, inkSoft, line } from "@/lib/tokens";
import { FIELD_LIMITS, MAX_IMAGE_BYTES, MAX_STEPS } from "../limits";

export type StepDraft = {
  key: string;
  id: number | null;
  text: string;
  file: File | null;
  preview: string | null;
  hadImage: boolean;
  removeImage: boolean;
};

type Props = {
  steps: StepDraft[];
  onChange: (steps: StepDraft[]) => void;
  nextKey: () => string;
  track: (url: string) => string;
};

const IMAGE_TYPES = ["image/png", "image/jpeg", "image/gif", "image/webp"];

export default function StepsEditor({ steps, onChange, nextKey, track }: Props) {
  const [notice, setNotice] = useState<string>();

  const update = (key: string, patch: Partial<StepDraft>) =>
    onChange(steps.map((step) => (step.key === key ? { ...step, ...patch } : step)));

  function move(index: number, offset: number) {
    const target = index + offset;
    if (target < 0 || target >= steps.length) return;

    const next = [...steps];
    [next[index], next[target]] = [next[target], next[index]];
    onChange(next);
  }

  function pick(key: string, file: File | undefined) {
    if (!file) return;

    if (!IMAGE_TYPES.includes(file.type)) {
      setNotice("A imagem precisa ser PNG, JPEG, GIF ou WebP.");
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      setNotice(`"${file.name}" passa do tamanho máximo de ${formatBytes(MAX_IMAGE_BYTES)}.`);
      return;
    }

    setNotice(undefined);
    update(key, { file, preview: track(URL.createObjectURL(file)), removeImage: false });
  }

  function dropImage(step: StepDraft) {
    update(step.key, { file: null, preview: null, removeImage: step.hadImage });
  }

  return (
    <Stack spacing={2}>
      {notice && <Alert severity="warning">{notice}</Alert>}

      {steps.length === 0 && <Typography sx={{ color: inkSoft }}>Nenhum passo ainda. Adicione o primeiro para montar o procedimento.</Typography>}

      {steps.map((step, index) => (
        <Box key={step.key} sx={{ border: `1px solid ${line}`, borderRadius: "12px", p: 2 }}>
          <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center", mb: 1.5 }}>
            <Typography sx={{ fontWeight: 600 }}>Passo {index + 1}</Typography>
            <Stack direction="row">
              <IconButton size="small" aria-label={`Subir o passo ${index + 1}`} disabled={index === 0} onClick={() => move(index, -1)}>
                <ArrowUpward fontSize="small" />
              </IconButton>
              <IconButton size="small" aria-label={`Descer o passo ${index + 1}`} disabled={index === steps.length - 1} onClick={() => move(index, 1)}>
                <ArrowDownward fontSize="small" />
              </IconButton>
              <IconButton size="small" aria-label={`Remover o passo ${index + 1}`} onClick={() => onChange(steps.filter((item) => item.key !== step.key))}>
                <Close fontSize="small" />
              </IconButton>
            </Stack>
          </Stack>

          <TextField
            label="Instrução"
            value={step.text}
            onChange={(e) => update(step.key, { text: e.target.value })}
            multiline
            minRows={2}
            slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.stepText } }}
            fullWidth
          />

          <Stack direction="row" spacing={1.5} sx={{ mt: 1.5, alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
            <Button component="label" size="small" variant="outlined">
              {step.preview ? "Trocar imagem" : "Adicionar imagem"}
              <input
                type="file"
                accept={IMAGE_TYPES.join(",")}
                hidden
                onChange={(e) => {
                  pick(step.key, e.target.files?.[0]);
                  e.target.value = "";
                }}
              />
            </Button>
            {step.preview && (
              <Button size="small" color="error" onClick={() => dropImage(step)}>
                Remover imagem
              </Button>
            )}
          </Stack>

          {step.preview && (
            <Box
              component="img"
              src={step.preview}
              alt={`Imagem do passo ${index + 1}`}
              sx={{ mt: 1.5, maxWidth: "100%", maxHeight: 220, borderRadius: "8px", border: `1px solid ${line}`, display: "block" }}
            />
          )}
        </Box>
      ))}

      <Box>
        <Button
          startIcon={<Add />}
          variant="outlined"
          disabled={steps.length >= MAX_STEPS}
          onClick={() => onChange([...steps, { key: nextKey(), id: null, text: "", file: null, preview: null, hadImage: false, removeImage: false }])}
          sx={{ borderColor: brand }}
        >
          Adicionar passo
        </Button>
      </Box>
    </Stack>
  );
}
