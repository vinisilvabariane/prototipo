import { useSyncExternalStore } from "react";

export type HubPrefs = { pinned: string[]; recent: string[] };

export const MAX_PINNED = 8;
const MAX_RECENT = 6;
const STORAGE_KEY = "meone.hub";
const EMPTY: HubPrefs = { pinned: [], recent: [] };

const listeners = new Set<() => void>();
let lastRaw: string | null | undefined;
let lastValue: HubPrefs = EMPTY;
let storageFailed = false;

const strings = (value: unknown) =>
  Array.isArray(value) ? value.filter((item): item is string => typeof item === "string") : [];

function parse(raw: string | null): HubPrefs {
  if (!raw) return EMPTY;
  try {
    const data = JSON.parse(raw);
    return { pinned: strings(data.pinned).slice(0, MAX_PINNED), recent: strings(data.recent).slice(0, MAX_RECENT) };
  } catch {
    return EMPTY;
  }
}

function snapshot(): HubPrefs {
  if (storageFailed) return lastValue;
  let raw: string | null;
  try {
    raw = window.localStorage.getItem(STORAGE_KEY);
  } catch {
    storageFailed = true;
    return lastValue;
  }
  if (raw !== lastRaw) {
    lastRaw = raw;
    lastValue = parse(raw);
  }
  return lastValue;
}

function save(next: HubPrefs) {
  lastValue = next;
  try {
    const raw = JSON.stringify(next);
    window.localStorage.setItem(STORAGE_KEY, raw);
    lastRaw = raw;
  } catch {
    storageFailed = true;
  }
  listeners.forEach((listener) => listener());
}

function subscribe(onChange: () => void) {
  listeners.add(onChange);
  window.addEventListener("storage", onChange);
  return () => {
    listeners.delete(onChange);
    window.removeEventListener("storage", onChange);
  };
}

export const useHubPrefs = () => useSyncExternalStore(subscribe, snapshot, () => EMPTY);

export function togglePin(href: string) {
  const { pinned, recent } = snapshot();
  if (pinned.includes(href)) return save({ pinned: pinned.filter((h) => h !== href), recent });
  if (pinned.length >= MAX_PINNED) return;
  save({ pinned: [...pinned, href], recent });
}

export function recordVisit(href: string) {
  const { pinned, recent } = snapshot();
  save({ pinned, recent: [href, ...recent.filter((h) => h !== href)].slice(0, MAX_RECENT) });
}
