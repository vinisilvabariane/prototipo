import { useSyncExternalStore } from "react";
import { COLOR_VISIONS, CONTRASTS, DEFAULT_PREFS, FONT_SIZES, LINE_HEIGHTS, MOTIONS, PREFS_STORAGE_KEY, THEME_NAMES, type Prefs } from "./options";

export { DEFAULT_PREFS, type Prefs } from "./options";

const listeners = new Set<() => void>();
let lastRaw: string | null | undefined;
let lastValue: Prefs = DEFAULT_PREFS;
let storageFailed = false;

const pick = <T extends string>(value: unknown, options: readonly T[], fallback: T): T =>
  options.includes(value as T) ? (value as T) : fallback;

const LEGACY_THEMES: Record<string, string> = { grafite: "graphite", oceano: "ocean", saturado: "vivid", leitura: "cream" };

export function parsePrefs(raw: string | null): Prefs {
  if (!raw) return DEFAULT_PREFS;
  try {
    const data = JSON.parse(raw) as Record<string, unknown>;
    return {
      theme: pick(LEGACY_THEMES[String(data.theme)] ?? data.theme, THEME_NAMES, DEFAULT_PREFS.theme),
      fontSize: pick(data.fontSize, FONT_SIZES, DEFAULT_PREFS.fontSize),
      lineHeight: pick(data.lineHeight, LINE_HEIGHTS, DEFAULT_PREFS.lineHeight),
      motion: pick(data.motion, MOTIONS, DEFAULT_PREFS.motion),
      contrast: pick(data.contrast, CONTRASTS, DEFAULT_PREFS.contrast),
      colorVision: pick(data.colorVision, COLOR_VISIONS, DEFAULT_PREFS.colorVision),
      libras: data.libras === true,
    };
  } catch {
    return DEFAULT_PREFS;
  }
}

function snapshot(): Prefs {
  if (storageFailed) return lastValue;
  let raw: string | null;
  try {
    raw = window.localStorage.getItem(PREFS_STORAGE_KEY);
  } catch {
    storageFailed = true;
    return lastValue;
  }
  if (raw !== lastRaw) {
    lastRaw = raw;
    lastValue = parsePrefs(raw);
  }
  return lastValue;
}

function save(next: Prefs) {
  lastValue = next;
  try {
    const raw = JSON.stringify(next);
    window.localStorage.setItem(PREFS_STORAGE_KEY, raw);
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

export const usePrefs = () => useSyncExternalStore(subscribe, snapshot, () => DEFAULT_PREFS);

export function setPref<K extends keyof Prefs>(key: K, value: Prefs[K]) {
  save({ ...snapshot(), [key]: value });
}

export function resetPrefs() {
  save({ ...DEFAULT_PREFS, theme: snapshot().theme });
}

export function applyPrefs(prefs: Prefs, root: HTMLElement = document.documentElement) {
  root.dataset.theme = prefs.theme;
  root.dataset.fontSize = prefs.fontSize;
  root.dataset.lineHeight = prefs.lineHeight;
  root.dataset.motion = prefs.motion;
  root.dataset.contrast = prefs.contrast;
  root.dataset.colorVision = prefs.colorVision;
}
