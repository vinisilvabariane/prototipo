import { createTheme } from "@mui/material/styles";
import {
  appFont,
  channel,
  cssVariables,
  inkSoft,
  themes,
  THEME_NAMES,
  type SchemeTokens,
  type ThemeDefinition,
} from "./tokens";

const palette = (scheme: SchemeTokens, semantic: ThemeDefinition["semantic"]) => ({
  primary: { main: scheme.brand, dark: scheme.brandDeep, contrastText: scheme.onInk },
  success: { main: scheme.brand },
  info: { main: scheme.brand },
  warning: { main: semantic.warning },
  error: { main: semantic.error },
  background: { default: scheme.canvas, paper: scheme.surface },
  text: { primary: scheme.ink, secondary: scheme.inkSoft },
  divider: scheme.line,
});

const semanticFor = (name: "error" | "warning", main: string, base: ThemeDefinition["base"], canvas: string): Record<string, string> => {
  const dark = base === "dark";
  return {
    [`--mui-palette-${name}-main`]: main,
    [`--mui-palette-${name}-mainChannel`]: channel(main),
    [`--mui-palette-${name}-light`]: main,
    [`--mui-palette-${name}-dark`]: main,
    [`--mui-palette-${name}-contrastText`]: dark ? "#000000" : "#FFFFFF",
    [`--mui-palette-Alert-${name}Color`]: dark ? `color-mix(in srgb, ${main} 70%, #FFFFFF)` : `color-mix(in srgb, ${main} 70%, #000000)`,
    [`--mui-palette-Alert-${name}StandardBg`]: dark ? `color-mix(in srgb, ${main} 18%, ${canvas})` : `color-mix(in srgb, ${main} 10%, #FFFFFF)`,
    [`--mui-palette-Alert-${name}IconColor`]: main,
    [`--mui-palette-Alert-${name}FilledBg`]: main,
  };
};

const themeVariables = ({ tokens, semantic, base }: ThemeDefinition): Record<string, string> => ({
  ...cssVariables(tokens),
  ...semanticFor("error", semantic.error, base, tokens.canvas),
  ...semanticFor("warning", semantic.warning, base, tokens.canvas),
  "--mui-palette-primary-main": tokens.brand,
  "--mui-palette-primary-mainChannel": channel(tokens.brand),
  "--mui-palette-primary-dark": tokens.brandDeep,
  "--mui-palette-primary-light": tokens.brand,
  "--mui-palette-primary-contrastText": tokens.onInk,
  "--mui-palette-success-main": tokens.brand,
  "--mui-palette-info-main": tokens.brand,
  "--mui-palette-background-default": tokens.canvas,
  "--mui-palette-background-paper": tokens.surface,
  "--mui-palette-text-primary": tokens.ink,
  "--mui-palette-text-secondary": tokens.inkSoft,
  "--mui-palette-divider": tokens.line,
});

const contrastVariables = (light: boolean): Record<string, string> =>
  light
    ? {
        "--mo-ink": "#000000",
        "--mo-ink-soft": "#1F2A44",
        "--mo-line": "#5A6480",
        "--mo-line-strong": "#0A1330",
        "--mo-brand": "#0F2BB0",
        "--mo-brand-deep": "#0A1F80",
        "--mui-palette-text-primary": "#000000",
        "--mui-palette-text-secondary": "#1F2A44",
        "--mui-palette-divider": "#5A6480",
        "--mui-palette-primary-main": "#0F2BB0",
        "--mui-palette-primary-contrastText": "#FFFFFF",
      }
    : {
        "--mo-ink": "#FFFFFF",
        "--mo-ink-soft": "#D5DCEF",
        "--mo-line": "#8A95B5",
        "--mo-line-strong": "#E8ECF8",
        "--mo-brand": "#B9C7FF",
        "--mo-brand-deep": "#D6DEFF",
        "--mui-palette-text-primary": "#FFFFFF",
        "--mui-palette-text-secondary": "#D5DCEF",
        "--mui-palette-divider": "#8A95B5",
        "--mui-palette-primary-main": "#B9C7FF",
        "--mui-palette-primary-contrastText": "#0B1222",
      };

type SemanticSet = Record<"error" | "warning" | "success", [string, string, string, string, string, string]>;

const colorVisionSets: Record<string, { light: SemanticSet; dark: SemanticSet }> = {
  protanopia: {
    light: {
      error: ["#E8710A", "#F09A52", "#B45500", "#FFFFFF", "#7A3A00", "#FFF1E4"],
      warning: ["#D9B300", "#E8CB4D", "#A38600", "#2A1A00", "#5C4700", "#FFF8DC"],
      success: ["#0090A8", "#4DB8CC", "#00657A", "#FFFFFF", "#004657", "#E0F5F9"],
    },
    dark: {
      error: ["#FF9A3C", "#FFB266", "#E8710A", "#0B1222", "#FFD9B0", "#3A2410"],
      warning: ["#F5D061", "#F8DD8A", "#D9B300", "#0B1222", "#FFEDB8", "#3A3010"],
      success: ["#4DD2E8", "#80E0EF", "#0090A8", "#0B1222", "#B8EEF6", "#0E3038"],
    },
  },
  deuteranopia: {
    light: {
      error: ["#D55E00", "#E67E33", "#A84800", "#FFFFFF", "#7A3400", "#FFF1E6"],
      warning: ["#E69F00", "#F0B73F", "#A87400", "#2A1A00", "#5C4000", "#FFF7E0"],
      success: ["#009E73", "#33B18F", "#006E50", "#FFFFFF", "#004D38", "#E3F6F0"],
    },
    dark: {
      error: ["#FF8C42", "#FFA366", "#D55E00", "#0B1222", "#FFD1B3", "#3A2110"],
      warning: ["#F5C542", "#F8D470", "#E69F00", "#0B1222", "#FFE8A8", "#3A2E0A"],
      success: ["#3FD3A6", "#6FE0BD", "#009E73", "#0B1222", "#B8F0DD", "#0F3328"],
    },
  },
  tritanopia: {
    light: {
      error: ["#D32F2F", "#E57373", "#9A0007", "#FFFFFF", "#7F1D1D", "#FDECEA"],
      warning: ["#00838F", "#4FB3BF", "#005662", "#FFFFFF", "#004D57", "#E0F4F6"],
      success: ["#37474F", "#62727B", "#102027", "#FFFFFF", "#1F2A30", "#ECEFF1"],
    },
    dark: {
      error: ["#FF6B6B", "#FF8A80", "#D32F2F", "#0B1222", "#FFC4C4", "#3B1515"],
      warning: ["#4DD0E1", "#80DEEA", "#00838F", "#0B1222", "#B2EBF2", "#0E3338"],
      success: ["#B0BEC5", "#CFD8DC", "#78909C", "#0B1222", "#ECEFF1", "#263238"],
    },
  },
  achromatopsia: {
    light: {
      error: ["#1F1F1F", "#4A4A4A", "#000000", "#FFFFFF", "#1F1F1F", "#E8E8E8"],
      warning: ["#5A5A5A", "#8A8A8A", "#333333", "#FFFFFF", "#3A3A3A", "#F0F0F0"],
      success: ["#8A8A8A", "#B0B0B0", "#5A5A5A", "#000000", "#4A4A4A", "#F6F6F6"],
    },
    dark: {
      error: ["#F5F5F5", "#FFFFFF", "#BDBDBD", "#000000", "#F5F5F5", "#2A2A2A"],
      warning: ["#BDBDBD", "#E0E0E0", "#8A8A8A", "#000000", "#E0E0E0", "#1F1F1F"],
      success: ["#8A8A8A", "#A8A8A8", "#5A5A5A", "#000000", "#C4C4C4", "#181818"],
    },
  },
};

const semanticVariables = (set: SemanticSet): Record<string, string> =>
  Object.fromEntries(
    Object.entries(set).flatMap(([name, [main, lightTone, dark, contrast, alertText, alertBg]]) => [
      [`--mui-palette-${name}-main`, main],
      [`--mui-palette-${name}-mainChannel`, channel(main)],
      [`--mui-palette-${name}-light`, lightTone],
      [`--mui-palette-${name}-dark`, dark],
      [`--mui-palette-${name}-contrastText`, contrast],
      [`--mui-palette-Alert-${name}Color`, alertText],
      [`--mui-palette-Alert-${name}StandardBg`, alertBg],
      [`--mui-palette-Alert-${name}IconColor`, main],
      [`--mui-palette-Alert-${name}FilledBg`, main],
    ]),
  );

const globalVariables: Record<string, Record<string, string>> = {
  ":root": cssVariables(themes.graphite.tokens),
  ':root[data-mui-color-scheme="dark"]': cssVariables(themes.noir.tokens),
  ...Object.fromEntries(THEME_NAMES.map((name) => [`html:root[data-theme="${name}"]`, themeVariables(themes[name])])),
  'html:root[data-contrast="high"]': contrastVariables(true),
  'html:root[data-mui-color-scheme="dark"][data-contrast="high"]': contrastVariables(false),
  'html:root[data-contrast="high"] a:not([role="button"])': { textDecoration: "underline" },
  ...Object.fromEntries(
    Object.entries(colorVisionSets).flatMap(([kind, sets]) => [
      [`html:root[data-color-vision="${kind}"]`, semanticVariables(sets.light)],
      [`html:root[data-mui-color-scheme="dark"][data-color-vision="${kind}"]`, semanticVariables(sets.dark)],
    ]),
  ),
};

export const theme = createTheme({
  cssVariables: { colorSchemeSelector: "data-mui-color-scheme" },
  colorSchemes: {
    light: { palette: palette(themes.graphite.tokens, themes.graphite.semantic) },
    dark: { palette: palette(themes.noir.tokens, themes.noir.semantic) },
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: appFont,
    fontSize: 14,
    h4: { fontWeight: 600, fontSize: "1.65rem", lineHeight: 1.2, letterSpacing: "-0.025em" },
    h5: { fontWeight: 600, letterSpacing: "-0.02em" },
    h6: { fontWeight: 600, fontSize: "1.05rem", lineHeight: 1.25, letterSpacing: "-0.01em" },
    body1: { lineHeight: "var(--mo-line-height, 1.5)" },
    body2: { lineHeight: "var(--mo-line-height, 1.43)" },
    overline: { fontWeight: 600, fontSize: "0.7rem", letterSpacing: "0.12em", lineHeight: 1.4 },
    button: { textTransform: "none", fontWeight: 600 },
  },
  components: {
    MuiCssBaseline: { styleOverrides: globalVariables },
    MuiButton: { defaultProps: { disableElevation: true }, styleOverrides: { root: { borderRadius: 10 } } },
    MuiCard: {
      defaultProps: { variant: "outlined" },
      styleOverrides: { root: { borderRadius: 14, boxShadow: "none" } },
    },
    MuiTextField: { defaultProps: { size: "small" } },
    MuiChip: { styleOverrides: { root: { fontWeight: 500 }, sizeSmall: { height: 22, fontSize: "0.75rem", "& .MuiChip-label": { paddingInline: 9 } } } },
    MuiDialogContent: { styleOverrides: { root: { paddingTop: 8 } } },
    MuiTable: { defaultProps: { size: "small" } },
    MuiTableRow: {
      styleOverrides: {
        root: {
          "tbody > &:hover": { backgroundColor: "var(--mui-palette-action-hover)" },
          "tbody > &:last-child > td": { borderBottom: 0 },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: { padding: "10px 12px", verticalAlign: "middle", "&:first-of-type": { paddingLeft: 20 }, "&:last-of-type": { paddingRight: 20 } },
        head: {
          fontWeight: 600,
          fontSize: "0.7rem",
          letterSpacing: "0.07em",
          textTransform: "uppercase",
          color: inkSoft,
          whiteSpace: "nowrap",
          paddingTop: 12,
          paddingBottom: 12,
        },
      },
    },
  },
});
