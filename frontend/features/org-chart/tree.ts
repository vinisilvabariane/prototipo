import type { OrgNode, OrgPosition } from "./types";

export const MAX_DEPTH = 12;

const normalize = (value: string) =>
  value
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .trim();

export function buildTree(positions: OrgPosition[]): OrgNode[] {
  const nodes = new Map<number, OrgNode>(positions.map((position) => [position.id, { position, children: [] }]));
  const roots: OrgNode[] = [];

  for (const position of positions) {
    const node = nodes.get(position.id)!;
    const parent = position.parentId === null ? undefined : nodes.get(position.parentId);
    if (parent) parent.children.push(node);
    else roots.push(node);
  }

  return roots;
}

export function indexById(positions: OrgPosition[]) {
  return new Map(positions.map((position) => [position.id, position]));
}

export function chainOf(byId: Map<number, OrgPosition>, id: number): OrgPosition[] {
  const chain: OrgPosition[] = [];
  let current = byId.get(id);

  while (current && chain.length <= byId.size) {
    chain.unshift(current);
    current = current.parentId === null ? undefined : byId.get(current.parentId);
  }

  return chain;
}

export function directReports(positions: OrgPosition[], id: number) {
  return positions.filter((position) => position.parentId === id);
}

export function descendantIds(positions: OrgPosition[], id: number) {
  const found = new Set<number>();
  const pending = [id];

  while (pending.length > 0) {
    const current = pending.pop()!;
    for (const position of positions) {
      if (position.parentId === current && !found.has(position.id)) {
        found.add(position.id);
        pending.push(position.id);
      }
    }
  }

  return found;
}

export function countBelow(node: OrgNode): number {
  return node.children.reduce((total, child) => total + 1 + countBelow(child), 0);
}

export function heightOf(positions: OrgPosition[], id: number): number {
  const children = positions.filter((position) => position.parentId === id);
  return 1 + children.reduce((tallest, child) => Math.max(tallest, heightOf(positions, child.id)), 0);
}

export function depthOf(byId: Map<number, OrgPosition>, id: number | null) {
  return id === null ? 0 : chainOf(byId, id).length;
}

export function allowedParents(positions: OrgPosition[], id: number) {
  const byId = indexById(positions);
  const blocked = descendantIds(positions, id);
  blocked.add(id);
  const height = heightOf(positions, id);

  return positions.filter((position) => !blocked.has(position.id) && depthOf(byId, position.id) + height <= MAX_DEPTH);
}

export function canAddBelow(positions: OrgPosition[], id: number) {
  return depthOf(indexById(positions), id) + 1 <= MAX_DEPTH;
}

export type Filter = { query: string; departmentId: number | null };

export const isFiltering = (filter: Filter) => normalize(filter.query) !== "" || filter.departmentId !== null;

export function matches(position: OrgPosition, filter: Filter) {
  if (filter.departmentId !== null && position.departmentId !== filter.departmentId) return false;

  const query = normalize(filter.query);
  if (!query) return true;

  const haystack = normalize(
    [position.title, position.department, position.holder?.name ?? "", position.holder ? String(position.holder.registration) : ""].join(" "),
  );

  return query.split(/\s+/).every((word) => haystack.includes(word));
}

export function matchingIds(positions: OrgPosition[], filter: Filter) {
  return new Set(positions.filter((position) => matches(position, filter)).map((position) => position.id));
}

export function ancestorsOf(byId: Map<number, OrgPosition>, ids: Set<number>) {
  const found = new Set<number>();

  for (const id of ids) {
    for (const position of chainOf(byId, id).slice(0, -1)) found.add(position.id);
  }

  return found;
}
