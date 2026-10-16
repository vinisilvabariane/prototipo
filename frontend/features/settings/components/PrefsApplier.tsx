"use client";

import { useEffect } from "react";
import { useColorScheme } from "@mui/material/styles";
import { applyPrefs, usePrefs } from "@/lib/prefs/store";
import { themes } from "@/lib/tokens";

export default function PrefsApplier() {
  const prefs = usePrefs();
  const { mode, setMode } = useColorScheme();
  const base = themes[prefs.theme].base;

  useEffect(() => {
    applyPrefs(prefs);
  }, [prefs]);

  useEffect(() => {
    if (mode !== undefined && mode !== base) setMode(base);
  }, [mode, base, setMode]);

  return null;
}
