"use client";

import { useState } from "react";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Stack from "@mui/material/Stack";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import Typography from "@mui/material/Typography";
import ConfirmDialog from "@/components/ui/ConfirmDialog";
import { inkSoft, line } from "@/lib/tokens";
import FormRenderer from "../form/FormRenderer";
import { moveSibling, removeOption, RESOLUTION_BLOCKED, siblingPosition } from "../schema";
import type { Answers, FieldTemplate, FieldType, FormField, Location } from "../types";
import Inspector from "./Inspector";
import OutlineTree from "./OutlineTree";
import TemplatesDialog from "./TemplatesDialog";
import { addChild, addRoot, applyTemplate, countDescendants, patchField, removeField, setId, setLabel, setType } from "./operations";

type Props = {
  fields: FormField[];
  onChange: (fields: FormField[]) => void;
  resolution?: boolean;
  templates: FieldTemplate[];
  onTemplates: (templates: FieldTemplate[]) => void;
  locations: Location[];
  intro: string;
};

export default function FormEditor({ fields, onChange, resolution, templates, onTemplates, locations, intro }: Props) {
  const [selectedId, setSelectedId] = useState<string | null>(fields[0]?.id ?? null);
  const [mode, setMode] = useState<"edit" | "preview">("edit");
  const [previewAnswers, setPreviewAnswers] = useState<Answers>({});
  const [managing, setManaging] = useState(false);
  const [removing, setRemoving] = useState(false);

  const blocked: FieldType[] = resolution ? RESOLUTION_BLOCKED : [];
  const selected = fields.find((field) => field.id === selectedId) ?? null;
  const parentField = selected?.dependsOn ? fields.find((field) => field.id === selected.dependsOn!.fieldId) : null;

  function commit(next: { fields: FormField[]; id: string }) {
    onChange(next.fields);
    setSelectedId(next.id);
  }

  function confirmRemove() {
    if (!selected) return;
    const next = removeField(fields, selected.id);
    onChange(next);
    setSelectedId(next[0]?.id ?? null);
    setRemoving(false);
  }

  const optionChildren = (value: string) =>
    selected ? fields.filter((field) => field.dependsOn?.fieldId === selected.id && field.dependsOn.values.includes(value)).length : 0;

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ justifyContent: "space-between", alignItems: { sm: "center" } }}>
        <Typography variant="body2" sx={{ color: inkSoft, maxWidth: 640 }}>
          {intro}
        </Typography>
        <ToggleButtonGroup exclusive size="small" value={mode} onChange={(_event, next: "edit" | "preview" | null) => next && setMode(next)} aria-label="Modo">
          <ToggleButton value="edit" sx={{ px: 2, textTransform: "none", fontWeight: 600 }}>
            Montar
          </ToggleButton>
          <ToggleButton value="preview" sx={{ px: 2, textTransform: "none", fontWeight: 600 }}>
            Pré-visualizar
          </ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      {mode === "preview" ? (
        <Card sx={{ p: { xs: 2, md: 3 }, maxWidth: 760 }}>
          <Typography variant="overline" component="p" sx={{ color: inkSoft, mb: 2 }}>
            Como a pessoa vai ver
          </Typography>
          <FormRenderer fields={fields} answers={previewAnswers} onChange={setPreviewAnswers} locations={locations} />
        </Card>
      ) : (
        <Box sx={{ display: "grid", gap: 2, alignItems: "start", gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(280px, 360px) minmax(0, 1fr)" } }}>
          <Card sx={{ p: 1.5 }}>
            <Typography variant="overline" component="h3" sx={{ color: inkSoft, display: "block", px: 0.5, pb: 1 }}>
              Perguntas · {fields.length}
            </Typography>
            <OutlineTree
              fields={fields}
              selectedId={selectedId}
              blocked={blocked}
              onSelect={setSelectedId}
              onAddRoot={(type) => commit(addRoot(fields, type))}
              onAddChild={(parentId, optionValue, type) => commit(addChild(fields, parentId, optionValue, type))}
            />
          </Card>

          <Card sx={{ p: { xs: 2, md: 2.5 }, minHeight: 200 }}>
            {selected ? (
              <Inspector
                key={selected.id}
                field={selected}
                fields={fields}
                blocked={blocked}
                templates={templates}
                position={siblingPosition(fields, selected.id)}
                descendants={countDescendants(fields, selected.id)}
                parent={
                  parentField && selected.dependsOn
                    ? { label: parentField.label, optionLabels: selected.dependsOn.values.map((value) => parentField.options.find((option) => option.value === value)?.label ?? value) }
                    : null
                }
                optionChildren={optionChildren}
                onLabel={(label) => commit(setLabel(fields, selected.id, label))}
                onId={(id) => commit(setId(fields, selected.id, id))}
                onType={(type) => onChange(setType(fields, selected.id, type))}
                onTemplate={(template) => onChange(applyTemplate(fields, selected.id, template))}
                onPatch={(patch) => onChange(patchField(fields, selected.id, patch))}
                onOptions={(options) => onChange(patchField(fields, selected.id, { options }))}
                onRemoveOption={(value) => onChange(removeOption(fields, selected.id, value))}
                onMove={(direction) => onChange(moveSibling(fields, selected.id, direction))}
                onRemove={() => (countDescendants(fields, selected.id) > 0 ? setRemoving(true) : confirmRemove())}
                onTemplateCreated={(template) => {
                  onTemplates([...templates, template].sort((a, b) => a.name.localeCompare(b.name)));
                  onChange(patchField(fields, selected.id, { templateId: template.id }));
                }}
                onManageTemplates={() => setManaging(true)}
              />
            ) : (
              <Typography variant="body2" sx={{ color: inkSoft, py: 4, textAlign: "center", borderTop: `1px dashed ${line}` }}>
                Escolha uma pergunta à esquerda para editar, ou adicione a primeira.
              </Typography>
            )}
          </Card>
        </Box>
      )}

      <TemplatesDialog
        open={managing}
        templates={templates}
        canUse={selected !== null}
        onClose={() => setManaging(false)}
        onUse={(template) => {
          if (selected) onChange(applyTemplate(fields, selected.id, template));
          setManaging(false);
        }}
        onDeleted={(id) => onTemplates(templates.filter((template) => template.id !== id))}
      />

      <ConfirmDialog
        open={removing}
        title="Remover pergunta e ramo?"
        text={selected ? `“${selected.label}” tem ${countDescendants(fields, selected.id)} pergunta(s) que dependem dela. Todas serão removidas.` : ""}
        confirmLabel="Remover"
        onCancel={() => setRemoving(false)}
        onConfirm={confirmRemove}
      />
    </Stack>
  );
}
