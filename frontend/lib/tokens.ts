export type SchemeTokens = {
  ink: string;
  inkSoft: string;
  surface: string;
  canvas: string;
  line: string;
  lineStrong: string;
  brand: string;
  brandDeep: string;
  brandWash: string;
  brandSoft: string;
  onInk: string;
  onInkSoft: string;
  headerBg: string;
  logoOutline: string;
};

export type ThemeBase = "light" | "dark";

export type ThemeDefinition = {
  label: string;
  hint: string;
  base: ThemeBase;
  tokens: SchemeTokens;
  semantic: { error: string; warning: string; success: string };
};

export const THEME_NAMES = ["graphite", "ocean", "noir", "vivid", "cream", "vulcan"] as const;
export type ThemeName = (typeof THEME_NAMES)[number];
export const DEFAULT_THEME: ThemeName = "graphite";

const lightSemantic = { error: "#C62E20", warning: "#9A6700", success: "#1E7B3E" };
const darkSemantic = { error: "#F2645A", warning: "#E0A800", success: "#4CC27A" };

export const themes: Record<ThemeName, ThemeDefinition> = {
  graphite: {
    label: "Graphite",
    hint: "Neutro, com azul nas ações",
    base: "light",
    semantic: lightSemantic,
    tokens: {
      ink: "#171A21",
      inkSoft: "#5B6270",
      surface: "#FFFFFF",
      canvas: "#F2F3F5",
      line: "#E1E4E9",
      lineStrong: "#C4C9D2",
      brand: "#2547D0",
      brandDeep: "#1E3A9E",
      brandWash: "#E4EAFD",
      brandSoft: "#EEF2FF",
      onInk: "#FFFFFF",
      onInkSoft: "rgba(255, 255, 255, 0.72)",
      headerBg: "rgba(255, 255, 255, 0.85)",
      logoOutline: "rgba(23, 26, 33, 0.35)",
    },
  },
  ocean: {
    label: "Ocean",
    hint: "Azul-petróleo e verde-água",
    base: "light",
    semantic: lightSemantic,
    tokens: {
      ink: "#0B2A3A",
      inkSoft: "#4F6D7E",
      surface: "#FFFFFF",
      canvas: "#EEF6F9",
      line: "#D6E6EE",
      lineStrong: "#B3CEDB",
      brand: "#0E7C9A",
      brandDeep: "#0A5F76",
      brandWash: "#D6EFF5",
      brandSoft: "#E7F6FA",
      onInk: "#FFFFFF",
      onInkSoft: "rgba(255, 255, 255, 0.72)",
      headerBg: "rgba(255, 255, 255, 0.85)",
      logoOutline: "rgba(11, 42, 58, 0.35)",
    },
  },
  noir: {
    label: "Noir",
    hint: "Escuro, quase preto",
    base: "dark",
    semantic: darkSemantic,
    tokens: {
      ink: "#F2F2F4",
      inkSoft: "#A0A0A8",
      surface: "#151518",
      canvas: "#0C0C0E",
      line: "#26262B",
      lineStrong: "#3A3A42",
      brand: "#E6E6EA",
      brandDeep: "#FFFFFF",
      brandWash: "#26262B",
      brandSoft: "#1C1C20",
      onInk: "#0C0C0E",
      onInkSoft: "rgba(12, 12, 14, 0.72)",
      headerBg: "rgba(12, 12, 14, 0.85)",
      logoOutline: "rgba(242, 242, 244, 0.4)",
    },
  },
  vivid: {
    label: "Vivid",
    hint: "Violeta vivo",
    base: "light",
    semantic: lightSemantic,
    tokens: {
      ink: "#1B0A3F",
      inkSoft: "#6A5A8F",
      surface: "#FFFFFF",
      canvas: "#FAF6FF",
      line: "#EADFFA",
      lineStrong: "#D3C1F2",
      brand: "#6E1FE8",
      brandDeep: "#5312B8",
      brandWash: "#EBDFFE",
      brandSoft: "#F3EBFF",
      onInk: "#FFFFFF",
      onInkSoft: "rgba(255, 255, 255, 0.72)",
      headerBg: "rgba(255, 255, 255, 0.85)",
      logoOutline: "rgba(27, 10, 63, 0.35)",
    },
  },
  cream: {
    label: "Cream",
    hint: "Tons de papel, para longas sessões",
    base: "light",
    semantic: lightSemantic,
    tokens: {
      ink: "#2B2418",
      inkSoft: "#6E6253",
      surface: "#FCF9F2",
      canvas: "#F3EDE1",
      line: "#E6DCC9",
      lineStrong: "#CFC2A9",
      brand: "#8A5A2B",
      brandDeep: "#6B4420",
      brandWash: "#EFE2CE",
      brandSoft: "#F5ECDC",
      onInk: "#FCF9F2",
      onInkSoft: "rgba(252, 249, 242, 0.72)",
      headerBg: "rgba(252, 249, 242, 0.85)",
      logoOutline: "rgba(43, 36, 24, 0.35)",
    },
  },
  vulcan: {
    label: "Vulcan",
    hint: "Escuro, com vermelho vivo",
    base: "dark",
    semantic: { error: "#FFB4A8", warning: "#E0A800", success: "#4CC27A" },
    tokens: {
      ink: "#F7ECE9",
      inkSoft: "#B59C97",
      surface: "#1E1212",
      canvas: "#130B0B",
      line: "#332020",
      lineStrong: "#4A2F2F",
      brand: "#E63E2A",
      brandDeep: "#FF6B4D",
      brandWash: "#3A1A14",
      brandSoft: "#2A1511",
      onInk: "#130B0B",
      onInkSoft: "rgba(19, 11, 11, 0.72)",
      headerBg: "rgba(19, 11, 11, 0.85)",
      logoOutline: "rgba(247, 236, 233, 0.4)",
    },
  },
};

const kebab = (name: string) => name.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);

export const cssVariables = (scheme: SchemeTokens): Record<string, string> =>
  Object.fromEntries(Object.entries(scheme).map(([name, value]) => [`--mo-${kebab(name)}`, value]));

export const channel = (hex: string) => {
  const value = hex.replace("#", "");
  return [0, 2, 4].map((i) => parseInt(value.slice(i, i + 2), 16)).join(" ");
};

const v = (name: keyof SchemeTokens) => `var(--mo-${kebab(name)})`;

export const ink = v("ink");
export const inkSoft = v("inkSoft");
export const surface = v("surface");
export const canvas = v("canvas");
export const line = v("line");
export const lineStrong = v("lineStrong");
export const brand = v("brand");
export const brandDeep = v("brandDeep");
export const brandWash = v("brandWash");
export const brandSoft = v("brandSoft");
export const onInk = v("onInk");
export const onInkSoft = v("onInkSoft");
export const headerBg = v("headerBg");
export const logoOutline = v("logoOutline");

export const appFont = "var(--font-google-sans), system-ui, sans-serif";

export const easeOut = "cubic-bezier(0.22, 1, 0.36, 1)";
