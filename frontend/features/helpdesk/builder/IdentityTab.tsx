"use client";

import Autocomplete from "@mui/material/Autocomplete";
import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import Chip from "@mui/material/Chip";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { brand, inkSoft, line } from "@/lib/tokens";
import SectorCard from "../components/SectorCard";
import { HelpdeskIcon, ICON_KEYS, ICON_LABELS } from "../icons";

export type Identity = {
  name: string;
  description: string;
  colorHex: string;
  icon: string;
  badges: string[];
  active: boolean;
  external: boolean;
  externalUrl: string;
};

type Props = { value: Identity; onChange: (value: Identity) => void; fieldCount: number };

const PRESETS = ["#2547D0", "#0E7C9A", "#1E7B3E", "#C26A00", "#C62E20", "#6E1FE8", "#8A5A2B", "#4B5563"];

export default function IdentityTab({ value, onChange, fieldCount }: Props) {
  const patch = (change: Partial<Identity>) => onChange({ ...value, ...change });

  return (
    <Box sx={{ display: "grid", gap: 3, alignItems: "start", gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "minmax(0, 1fr) 340px" } }}>
      <Stack spacing={2.5}>
        <TextField label="Nome do setor" value={value.name} onChange={(event) => patch({ name: event.target.value })} required fullWidth slotProps={{ htmlInput: { maxLength: 60 } }} />
        <TextField label="Descrição (aparece no card)" value={value.description} onChange={(event) => patch({ description: event.target.value })} fullWidth multiline minRows={2} slotProps={{ htmlInput: { maxLength: 200 } }} />

        <Box>
          <Typography variant="overline" component="h3" sx={{ color: inkSoft, display: "block", mb: 1 }}>
            Cor
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
            {PRESETS.map((color) => (
              <ButtonBase key={color} aria-label={`Cor ${color}`} aria-pressed={value.colorHex.toUpperCase() === color} onClick={() => patch({ colorHex: color })} sx={{ width: 32, height: 32, borderRadius: "50%", bgcolor: color, outline: value.colorHex.toUpperCase() === color ? `3px solid ${brand}` : `1px solid ${line}`, outlineOffset: 2 }} />
            ))}
            <Box component="input" type="color" aria-label="Outra cor" value={value.colorHex} onChange={(event) => patch({ colorHex: event.target.value.toUpperCase() })} sx={{ width: 36, height: 36, p: 0, border: `1px solid ${line}`, borderRadius: 1, bgcolor: "transparent", cursor: "pointer" }} />
          </Stack>
        </Box>

        <Box>
          <Typography variant="overline" component="h3" sx={{ color: inkSoft, display: "block", mb: 1 }}>
            Ícone
          </Typography>
          <Box sx={{ display: "grid", gap: 1, gridTemplateColumns: "repeat(auto-fill, minmax(88px, 1fr))" }}>
            {ICON_KEYS.map((key) => (
              <ButtonBase key={key} aria-pressed={value.icon === key} onClick={() => patch({ icon: key })} sx={{ flexDirection: "column", gap: 0.5, py: 1, borderRadius: 1.5, border: `1.5px solid ${value.icon === key ? value.colorHex : line}`, color: value.icon === key ? value.colorHex : inkSoft }}>
                <HelpdeskIcon name={key} />
                <Typography variant="caption" sx={{ fontWeight: 600 }}>
                  {ICON_LABELS[key]}
                </Typography>
              </ButtonBase>
            ))}
          </Box>
        </Box>

        <Autocomplete
          multiple
          freeSolo
          options={[] as string[]}
          value={value.badges}
          onChange={(_event, next) => patch({ badges: next.map((item) => item.trim()).filter(Boolean).slice(0, 5) })}
          renderValue={(items, getItemProps) => items.map((item, index) => <Chip variant="outlined" size="small" label={item} {...getItemProps({ index })} key={item} />)}
          renderInput={(params) => <TextField {...params} label="Etiquetas (até 5)" helperText="Digite e tecle Enter. Aparecem no card, como “Urgente” ou “Só dias úteis”." />}
        />

        <Stack spacing={1}>
          <FormControlLabel control={<Switch checked={value.active} onChange={(event) => patch({ active: event.target.checked })} />} label="Setor ativo (recebe chamados)" />
          <FormControlLabel control={<Switch checked={value.external} onChange={(event) => patch({ external: event.target.checked })} />} label="Setor externo (abre outro sistema)" />
          {value.external && (
            <TextField
              label="Endereço do outro sistema"
              value={value.externalUrl}
              onChange={(event) => patch({ externalUrl: event.target.value })}
              placeholder="https://"
              fullWidth
              helperText="O card abre esse endereço em outra aba. Um setor externo não tem formulário, resposta do técnico nem atendimento aqui."
            />
          )}
        </Stack>
      </Stack>

      <Box>
        <Typography variant="overline" component="h3" sx={{ color: inkSoft, display: "block", mb: 1 }}>
          Como o card aparece
        </Typography>
        <Box sx={{ pointerEvents: "none" }} aria-hidden>
          <SectorCard
            sector={{
              id: 0,
              name: value.name.trim() || "Nome do setor",
              description: value.description.trim() || null,
              colorHex: value.colorHex,
              icon: value.icon,
              badges: value.badges,
              active: value.active,
              externalUrl: value.external ? value.externalUrl || "https://" : null,
              fieldCount,
              canAttend: false,
            }}
          />
        </Box>
      </Box>
    </Box>
  );
}
