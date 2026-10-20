import type { Answers, FieldType, FormField, FormOption } from "./types";

export const FIELD_TYPES: FieldType[] = ["text", "textArea", "number", "select", "cardRadio", "productionSpot", "lineStop", "defects"];

export const FIELD_LABELS: Record<FieldType, string> = {
  text: "Texto curto",
  textArea: "Texto longo",
  number: "Número",
  select: "Lista suspensa",
  cardRadio: "Cards selecionáveis",
  productionSpot: "Linha e posto",
  lineStop: "Parada de linha",
  defects: "Peças com defeito",
};

export const FIELD_HINTS: Record<FieldType, string> = {
  text: "Uma linha de resposta.",
  textArea: "Resposta com várias linhas.",
  number: "Aceita decimais.",
  select: "Escolhe uma entre várias opções; cada opção pode abrir perguntas novas.",
  cardRadio: "Opções em cards grandes; cada card pode abrir perguntas novas.",
  productionSpot: "Escolhe a linha e, se houver produção em andamento, o posto. Vira localização do chamado.",
  lineStop: "Informa se a linha está parada e desde quando. Vira parada de disponibilidade no OEE.",
  defects: "Quantidade de peças com defeito. Vira defeito de qualidade no OEE.",
};

export const SPECIAL_TYPES: FieldType[] = ["productionSpot", "lineStop", "defects"];
export const RESOLUTION_BLOCKED: FieldType[] = SPECIAL_TYPES;

export const isChoice = (type: FieldType) => type === "select" || type === "cardRadio";
export const isSpecial = (type: FieldType) => SPECIAL_TYPES.includes(type);

export const POST_SUFFIX = "_post";
export const TIME_SUFFIX = "_time";

export function visibleFields(fields: FormField[], answers: Answers): FormField[] {
  const shown: FormField[] = [];
  const ids = new Set<string>();

  for (const field of fields) {
    const dependency = field.dependsOn;
    if (dependency && (!ids.has(dependency.fieldId) || !dependency.values.includes(answers[dependency.fieldId] ?? ""))) continue;
    shown.push(field);
    ids.add(field.id);
  }

  return shown;
}

export function firstProblem(fields: FormField[], answers: Answers): string | null {
  for (const field of visibleFields(fields, answers)) {
    if (field.required && !(answers[field.id] ?? "").trim()) return `Responda "${field.label}".`;
  }

  return null;
}

export type TreeBranch = { option: FormOption; children: TreeNode[]; orphan: boolean };
export type TreeNode = { field: FormField; branches: TreeBranch[] };

export function buildTree(fields: FormField[]): TreeNode[] {
  const ids = new Set(fields.map((field) => field.id));
  const childrenOf = new Map<string, FormField[]>();
  const roots: FormField[] = [];

  for (const field of fields) {
    const parent = field.dependsOn?.fieldId;
    if (parent && ids.has(parent)) childrenOf.set(parent, [...(childrenOf.get(parent) ?? []), field]);
    else roots.push(field);
  }

  const node = (field: FormField): TreeNode => {
    const byValue = new Map<string, FormField[]>();

    for (const child of childrenOf.get(field.id) ?? []) {
      for (const value of child.dependsOn?.values ?? [""]) byValue.set(value, [...(byValue.get(value) ?? []), child]);
    }

    const used = new Set<string>();
    const branches: TreeBranch[] = field.options.map((option) => {
      used.add(option.value);
      return { option, children: (byValue.get(option.value) ?? []).map(node), orphan: false };
    });

    for (const [value, group] of byValue) {
      if (!used.has(value)) branches.push({ option: { value, label: `Opção removida (${value})` }, children: group.map(node), orphan: true });
    }

    return { field, branches };
  };

  return roots.map(node);
}

export function flattenTree(nodes: TreeNode[]): FormField[] {
  const result: FormField[] = [];
  const seen = new Set<string>();

  const walk = (node: TreeNode) => {
    if (seen.has(node.field.id)) return;
    seen.add(node.field.id);
    result.push(node.field);
    for (const branch of node.branches) branch.children.forEach(walk);
  };

  nodes.forEach(walk);
  return result;
}

export const canonicalize = (fields: FormField[]) => flattenTree(buildTree(fields));

export function subtreeIds(fields: FormField[], rootId: string): Set<string> {
  const ids = new Set([rootId]);
  let changed = true;

  while (changed) {
    changed = false;
    for (const field of fields) {
      const parent = field.dependsOn?.fieldId;
      if (parent && ids.has(parent) && !ids.has(field.id)) {
        ids.add(field.id);
        changed = true;
      }
    }
  }

  return ids;
}

export function slugify(text: string) {
  const slug = text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "_")
    .replace(/^_+|_+$/g, "")
    .slice(0, 40);

  return /^[a-z]/.test(slug) ? slug : `campo_${slug}`.replace(/_$/, "");
}

export function uniqueId(base: string, taken: Iterable<string>) {
  const used = new Set(taken);
  const root = base || "campo";
  if (!used.has(root)) return root;

  let n = 2;
  while (used.has(`${root}_${n}`)) n++;
  return `${root}_${n}`;
}

export function newField(type: FieldType, fields: FormField[], dependsOn?: { fieldId: string; values: string[] }): FormField {
  const label = type === "text" ? "Nova pergunta" : FIELD_LABELS[type];
  const id = uniqueId(slugify(type === "text" ? "nova_pergunta" : label), fields.map((field) => field.id));

  return {
    id,
    type,
    label,
    required: type !== "defects",
    placeholder: null,
    options: isChoice(type) ? [{ value: "opcao_1", label: "Opção 1" }] : [],
    dependsOn: dependsOn ?? null,
    templateId: null,
  };
}

export function newOption(field: FormField): FormOption {
  const n = field.options.length + 1;
  return { value: uniqueId(`opcao_${n}`, field.options.map((option) => option.value)), label: `Opção ${n}` };
}

export function renameFieldId(fields: FormField[], oldId: string, nextId: string): FormField[] {
  return fields.map((field) => {
    if (field.id === oldId) return { ...field, id: nextId };
    if (field.dependsOn?.fieldId === oldId) return { ...field, dependsOn: { ...field.dependsOn, fieldId: nextId } };
    return field;
  });
}

export function removeOption(fields: FormField[], fieldId: string, value: string): FormField[] {
  const keep = fields.map((field) => (field.id === fieldId ? { ...field, options: field.options.filter((option) => option.value !== value) } : field));
  const drop = new Set<string>();

  for (const field of keep) {
    if (field.dependsOn?.fieldId !== fieldId || !field.dependsOn.values.includes(value)) continue;
    const remaining = field.dependsOn.values.filter((item) => item !== value);
    if (remaining.length === 0) for (const id of subtreeIds(keep, field.id)) drop.add(id);
  }

  return keep
    .filter((field) => !drop.has(field.id))
    .map((field) =>
      field.dependsOn?.fieldId === fieldId && field.dependsOn.values.includes(value)
        ? { ...field, dependsOn: { ...field.dependsOn, values: field.dependsOn.values.filter((item) => item !== value) } }
        : field,
    );
}

export function moveSibling(fields: FormField[], fieldId: string, direction: -1 | 1): FormField[] {
  const tree = buildTree(fields);
  const move = (nodes: TreeNode[]): TreeNode[] => {
    const index = nodes.findIndex((node) => node.field.id === fieldId);

    if (index >= 0) {
      const target = index + direction;
      if (target < 0 || target >= nodes.length) return nodes;
      const copy = [...nodes];
      [copy[index], copy[target]] = [copy[target], copy[index]];
      return copy;
    }

    return nodes.map((node) => ({ ...node, branches: node.branches.map((branch) => ({ ...branch, children: move(branch.children) })) }));
  };

  return flattenTree(move(tree));
}

export function siblingPosition(fields: FormField[], fieldId: string): { index: number; count: number } {
  const find = (nodes: TreeNode[]): { index: number; count: number } | null => {
    const index = nodes.findIndex((node) => node.field.id === fieldId);
    if (index >= 0) return { index, count: nodes.length };

    for (const node of nodes) {
      for (const branch of node.branches) {
        const found = find(branch.children);
        if (found) return found;
      }
    }

    return null;
  };

  return find(buildTree(fields)) ?? { index: 0, count: 1 };
}
