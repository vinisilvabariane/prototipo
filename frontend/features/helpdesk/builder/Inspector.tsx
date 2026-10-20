"use client";

import { useState, useTransition } from "react";
import ArrowDownward from "@mui/icons-material/ArrowDownward";
import ArrowUpward from "@mui/icons-material/ArrowUpward";
import DeleteOutlined from "@mui/icons-material/DeleteOutlined";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import FormControlLabel from "@mui/material/FormControlLabel";
import IconButton from "@mui/material/IconButton";
import ListSubheader from "@mui/material/ListSubheader";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft, line } from "@/lib/tokens";
import { createTemplate } from "../actions/sectorActions";
import { FIELD_HINTS, FIELD_LABELS, FIELD_TYPES, isChoice, isSpecial } from "../schema";
import type { FieldTemplate, FieldType, FormField, FormOption } from "../types";
import OptionsEditor from "./OptionsEditor";

const TEMPLATE_PREFIX = "tpl:";

type Props = {
  field: FormField;
  fields: FormField[];
  blocked: FieldType[];
  templates: FieldTemplate[];
  position: { index: number; count: number };
  descendants: number;
  parent: { label: string; optionLabels: string[] } | null;
  onLabel: (label: string) => void;
  onId: (id: string) => void;
  onType: (type: FieldType) => void;
  onTemplate: (template: FieldTemplate) => void;
  onPatch: (patch: Partial<FormField>) => void;
  onOptions: (options: FormOption[]) => void;
  onRemoveOption: (value: string) => void;
  onMove: (direction: -1 | 1) => void;
  onRemove: () => void;
  onTemplateCreated: (template: FieldTemplate) => void;
  onManageTemplates: () => void;
  optionChildren: (value: string) => number;
};

export default function Inspector(props: Props) {
  const { field, fields, blocked, templates, position, descendants, parent } = props;
  const [saving, setSaving] = useState(false);
  const [templateName, setTemplateName] = useState("");
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  const usedSpecials = new Set(fields.filter((item) => item.id !== field.id && isSpecial(item.type)).map((item) => item.type));
  const linked = templates.find((template) => template.id === field.templateId);
  const typeValue = linked ? `${TEMPLATE_PREFIX}${linked.id}` : field.type;
  const hasPlaceholder = field.type === "text" || field.type === "textArea" || field.type === "number";

  function changeType(value: string) {
    if (value.startsWith(TEMPLATE_PREFIX)) {
      const template = templates.find((item) => item.id === Number(value.slice(TEMPLATE_PREFIX.length)));
      if (template) props.onTemplate(template);
      return;
    }

    props.onType(value as FieldType);
  }

  function saveTemplate() {
    setError(undefined);

    startTransition(async () => {
      const result = await createTemplate({ name: templateName.trim(), type: field.type, label: field.label, required: field.required, options: field.options });

      if (!result.ok) {
        setError(result.error);
        return;
      }

      props.onTemplateCreated(result.value);
      setSaving(false);
      setTemplateName("");
    });
  }

  return (
    <Stack spacing={2.5}>
      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        <Typography variant="overline" component="h3" sx={{ color: inkSoft, flex: 1 }}>
          Pergunta · {FIELD_LABELS[field.type]}
        </Typography>
        <Tooltip title="Subir">
          <span>
            <IconButton size="small" onClick={() => props.onMove(-1)} disabled={position.index === 0} aria-label="Subir pergunta">
              <ArrowUpward fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>
        <Tooltip title="Descer">
          <span>
            <IconButton size="small" onClick={() => props.onMove(1)} disabled={position.index >= position.count - 1} aria-label="Descer pergunta">
              <ArrowDownward fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>
        <Tooltip title={descendants > 0 ? `Remove a pergunta e as ${descendants} que dependem dela` : "Remover pergunta"}>
          <IconButton size="small" color="error" onClick={props.onRemove} aria-label="Remover pergunta">
            <DeleteOutlined fontSize="small" />
          </IconButton>
        </Tooltip>
      </Stack>

      {parent && (
        <Alert severity="info" variant="outlined" icon={false}>
          Aparece quando <strong>{parent.label}</strong> for {parent.optionLabels.map((label) => `«${label}»`).join(" ou ")}.
        </Alert>
      )}

      <TextField select label="Tipo" value={typeValue} onChange={(event) => changeType(event.target.value)} helperText={linked ? `Veio do modelo “${linked.name}”.` : FIELD_HINTS[field.type]}>
        {FIELD_TYPES.filter((type) => !blocked.includes(type)).map((type) => (
          <MenuItem key={type} value={type} disabled={isSpecial(type) && usedSpecials.has(type)}>
            {FIELD_LABELS[type]}
            {isSpecial(type) && usedSpecials.has(type) && " (já usado)"}
          </MenuItem>
        ))}
        {templates.length > 0 && <ListSubheader>Modelos salvos</ListSubheader>}
        {templates.map((template) => (
          <MenuItem key={template.id} value={`${TEMPLATE_PREFIX}${template.id}`}>
            {template.name}
          </MenuItem>
        ))}
      </TextField>

      <TextField label="Título da pergunta" value={field.label} onChange={(event) => props.onLabel(event.target.value)} slotProps={{ htmlInput: { maxLength: 200 } }} />

      <FormControlLabel control={<Switch checked={field.required} onChange={(event) => props.onPatch({ required: event.target.checked })} />} label="Resposta obrigatória" />

      {hasPlaceholder && <TextField label="Dica dentro do campo (opcional)" value={field.placeholder ?? ""} onChange={(event) => props.onPatch({ placeholder: event.target.value || null })} slotProps={{ htmlInput: { maxLength: 200 } }} />}

      {isChoice(field.type) && <OptionsEditor field={field} childCount={props.optionChildren} onChange={props.onOptions} onRemove={props.onRemoveOption} />}

      {field.type === "productionSpot" && <Alert severity="info" variant="outlined">A linha escolhida (e o posto, se houver produção em andamento) vira a localização do chamado.</Alert>}
      {field.type === "lineStop" && <Alert severity="warning" variant="outlined">Se a pessoa marcar “Parada”, o tempo até o chamado ser resolvido entra como perda de disponibilidade no OEE do posto ou da linha.</Alert>}
      {field.type === "defects" && <Alert severity="info" variant="outlined">A quantidade informada entra como defeito de qualidade no OEE do posto ou da linha.</Alert>}

      <Box sx={{ borderTop: `1px solid ${line}`, pt: 2 }}>
        <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", rowGap: 1, alignItems: "center" }}>
          {!isSpecial(field.type) && (
            <Button size="small" variant="outlined" onClick={() => setSaving(true)}>
              Salvar como modelo
            </Button>
          )}
          <Button size="small" onClick={props.onManageTemplates}>
            Gerenciar modelos
          </Button>
          {linked && <Chip size="small" variant="outlined" label={`Modelo: ${linked.name}`} />}
        </Stack>
        <TextField label="Identificador" size="small" value={field.id} onChange={(event) => props.onId(event.target.value)} fullWidth sx={{ mt: 2 }} helperText="Nome interno usado nas respostas e nas integrações. Acompanha o título enquanto não for editado à mão." />
      </Box>

      <Dialog open={saving} onClose={() => setSaving(false)} fullWidth maxWidth="xs">
        <DialogTitle>Salvar como modelo</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {error && <ErrorAlert error={error} />}
            <TextField label="Nome do modelo" value={templateName} onChange={(event) => setTemplateName(event.target.value)} autoFocus fullWidth slotProps={{ htmlInput: { maxLength: 60 } }} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSaving(false)}>Cancelar</Button>
          <Button variant="contained" disabled={pending || !templateName.trim()} onClick={saveTemplate}>
            {pending ? "Salvando..." : "Salvar modelo"}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
