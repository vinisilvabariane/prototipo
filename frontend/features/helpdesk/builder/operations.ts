import { canonicalize, FIELD_LABELS, isChoice, newField, renameFieldId, slugify, subtreeIds, uniqueId } from "../schema";
import type { FieldTemplate, FieldType, FormField } from "../types";

const otherIds = (fields: FormField[], id: string) => fields.filter((field) => field.id !== id).map((field) => field.id);

export function addRoot(fields: FormField[], type: FieldType): { fields: FormField[]; id: string } {
  const created = newField(type, fields);
  return { fields: canonicalize([...fields, created]), id: created.id };
}

export function addChild(fields: FormField[], parentId: string, optionValue: string, type: FieldType): { fields: FormField[]; id: string } {
  const created = newField(type, fields, { fieldId: parentId, values: [optionValue] });
  return { fields: canonicalize([...fields, created]), id: created.id };
}

export function removeField(fields: FormField[], id: string): FormField[] {
  const gone = subtreeIds(fields, id);
  return fields.filter((field) => !gone.has(field.id));
}

export function patchField(fields: FormField[], id: string, patch: Partial<FormField>): FormField[] {
  return fields.map((field) => (field.id === id ? { ...field, ...patch } : field));
}

export function setLabel(fields: FormField[], id: string, label: string): { fields: FormField[]; id: string } {
  const current = fields.find((field) => field.id === id);
  if (!current) return { fields, id };

  const taken = otherIds(fields, id);
  const synced = current.id === uniqueId(slugify(current.label), taken);
  const patched = patchField(fields, id, { label });

  if (!synced || !label.trim()) return { fields: patched, id };

  const nextId = uniqueId(slugify(label), taken);
  return nextId === id ? { fields: patched, id } : { fields: renameFieldId(patched, id, nextId), id: nextId };
}

export function setId(fields: FormField[], id: string, raw: string): { fields: FormField[]; id: string } {
  const cleaned = raw.toLowerCase().replace(/[^a-z0-9_]+/g, "_").replace(/^[^a-z]+/, "").slice(0, 50);
  if (!cleaned || cleaned === id) return { fields, id };

  const nextId = uniqueId(cleaned, otherIds(fields, id));
  return { fields: renameFieldId(fields, id, nextId), id: nextId };
}

export function setType(fields: FormField[], id: string, type: FieldType): FormField[] {
  const current = fields.find((field) => field.id === id);
  if (!current || current.type === type) return fields;

  let next = fields;

  if (isChoice(current.type) && !isChoice(type)) {
    const children = fields.filter((field) => field.dependsOn?.fieldId === id).map((field) => field.id);
    const gone = new Set(children.flatMap((child) => [...subtreeIds(fields, child)]));
    next = fields.filter((field) => !gone.has(field.id));
  }

  const keepsLabel = current.label !== FIELD_LABELS[current.type] && current.label !== "Nova pergunta";

  return patchField(next, id, {
    type,
    templateId: null,
    options: isChoice(type) ? (isChoice(current.type) && current.options.length > 0 ? current.options : [{ value: "opcao_1", label: "Opção 1" }]) : [],
    label: keepsLabel ? current.label : type === "text" ? "Nova pergunta" : FIELD_LABELS[type],
    required: type === "defects" ? false : current.required,
  });
}

export function applyTemplate(fields: FormField[], id: string, template: FieldTemplate): FormField[] {
  const current = fields.find((field) => field.id === id);
  if (!current) return fields;

  const children = isChoice(current.type) && !isChoice(template.type) ? fields.filter((field) => field.dependsOn?.fieldId === id).map((field) => field.id) : [];
  const gone = new Set(children.flatMap((child) => [...subtreeIds(fields, child)]));
  const kept = fields.filter((field) => !gone.has(field.id));

  return patchField(kept, id, {
    type: template.type,
    label: template.label,
    required: template.required,
    options: template.options.map((option) => ({ ...option })),
    templateId: template.id,
  });
}

export function countDescendants(fields: FormField[], id: string) {
  return subtreeIds(fields, id).size - 1;
}
