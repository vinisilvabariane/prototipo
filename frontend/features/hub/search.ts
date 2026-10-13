import { allEntries, isLinkable, type HubEntry, type HubModule } from "./modules";

const normalize = (value: string) =>
  value
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .trim();

const containsAll = (haystack: string, query: string) =>
  query.split(/\s+/).every((word) => haystack.includes(word));

export function moduleMatches(module: HubModule, rawQuery: string): boolean {
  const query = normalize(rawQuery);
  if (!query) return true;

  const haystack = normalize(
    [module.code, module.name, module.description, ...module.functions.map((fn) => fn.label)].join(" "),
  );
  return containsAll(haystack, query);
}

export function searchEntries(rawQuery: string, limit = 10): HubEntry[] {
  const query = normalize(rawQuery);
  const matches = query
    ? allEntries.filter((entry) =>
        containsAll(normalize([entry.fn.label, entry.module.name, entry.module.code, entry.group.name].join(" ")), query),
      )
    : allEntries;

  const available = matches.filter(isLinkable);
  const others = matches.filter((entry) => !isLinkable(entry));
  return [...available, ...others].slice(0, limit);
}
