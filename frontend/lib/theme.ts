import { createTheme } from "@mui/material/styles";
import { bodyFont, brand, canvas, displayFont, ink, inkSoft, line, surface } from "./tokens";

export const theme = createTheme({
  cssVariables: true,
  palette: {
    primary: { main: brand, contrastText: "#FFFFFF" },
    success: { main: brand },
    warning: { main: "#9A6700" },
    error: { main: "#C62E20" },
    info: { main: brand },
    background: { default: canvas, paper: surface },
    text: { primary: ink, secondary: inkSoft },
    divider: line,
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: bodyFont,
    fontSize: 14,
    h1: { fontFamily: displayFont },
    h2: { fontFamily: displayFont },
    h3: { fontFamily: displayFont },
    h4: { fontFamily: displayFont, fontWeight: 600, fontSize: "2.1rem", lineHeight: 1.15, letterSpacing: "-0.03em" },
    h5: { fontFamily: displayFont, fontWeight: 600, letterSpacing: "-0.02em" },
    h6: { fontFamily: displayFont, fontWeight: 600, fontSize: "1.05rem", lineHeight: 1.25, letterSpacing: "-0.01em" },
    overline: { fontFamily: bodyFont, fontWeight: 600, fontSize: "0.7rem", letterSpacing: "0.12em", lineHeight: 1.4 },
    button: { textTransform: "none", fontWeight: 600 },
  },
  components: {
    MuiCssBaseline: { styleOverrides: { body: { backgroundColor: canvas } } },
    MuiButton: { defaultProps: { disableElevation: true }, styleOverrides: { root: { borderRadius: 10 } } },
    MuiCard: {
      defaultProps: { variant: "outlined" },
      styleOverrides: { root: { borderRadius: 14, boxShadow: "none" } },
    },
    MuiTable: { defaultProps: { size: "small" } },
    MuiTableCell: {
      styleOverrides: {
        head: {
          fontWeight: 600,
          fontSize: "0.7rem",
          letterSpacing: "0.1em",
          textTransform: "uppercase",
          color: inkSoft,
        },
      },
    },
  },
});
