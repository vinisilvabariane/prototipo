import { DEFAULT_THEME, THEME_NAMES, type ThemeName } from "@/lib/tokens";

export const PREFS_STORAGE_KEY = "meone.prefs";

export { THEME_NAMES };
export const FONT_SIZES = ["sm", "md", "lg", "xl"] as const;
export const LINE_HEIGHTS = ["normal", "relaxed"] as const;
export const MOTIONS = ["full", "reduced"] as const;
export const CONTRASTS = ["normal", "high"] as const;
export const COLOR_VISIONS = ["normal", "protanopia", "deuteranopia", "tritanopia", "achromatopsia"] as const;

export type Prefs = {
  theme: ThemeName;
  fontSize: (typeof FONT_SIZES)[number];
  lineHeight: (typeof LINE_HEIGHTS)[number];
  motion: (typeof MOTIONS)[number];
  contrast: (typeof CONTRASTS)[number];
  colorVision: (typeof COLOR_VISIONS)[number];
  libras: boolean;
};

export const DEFAULT_PREFS: Prefs = {
  theme: DEFAULT_THEME,
  fontSize: "md",
  lineHeight: "normal",
  motion: "full",
  contrast: "normal",
  colorVision: "normal",
  libras: false,
};
