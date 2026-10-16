"use client";

import { useEffect } from "react";
import { usePrefs } from "@/lib/prefs/store";

const SCRIPT_ID = "vlibras-plugin";
const SCRIPT_URL = "https://vlibras.gov.br/app/vlibras-plugin.js";
const APP_URL = "https://vlibras.gov.br/app";

declare global {
  interface Window {
    VLibras?: { Widget: new (appUrl: string) => unknown };
  }
}

function mountContainer() {
  const container = document.createElement("div");
  container.setAttribute("vw", "");
  container.className = "enabled";
  container.innerHTML =
    '<div vw-access-button class="active"></div><div vw-plugin-wrapper><div class="vw-plugin-top-wrapper"></div></div>';
  document.body.appendChild(container);
  return container;
}

function loadScript(): Promise<void> {
  return new Promise((resolve, reject) => {
    if (window.VLibras) return resolve();
    const existing = document.getElementById(SCRIPT_ID) as HTMLScriptElement | null;
    const script = existing ?? document.createElement("script");
    script.addEventListener("load", () => resolve(), { once: true });
    script.addEventListener("error", () => reject(new Error("VLibras não carregou")), { once: true });
    if (!existing) {
      script.id = SCRIPT_ID;
      script.src = SCRIPT_URL;
      script.async = true;
      document.body.appendChild(script);
    }
  });
}

export default function LibrasWidget() {
  const { libras } = usePrefs();

  useEffect(() => {
    if (!libras) return;

    let container: HTMLElement | null = null;
    let cancelled = false;

    loadScript()
      .then(() => {
        if (cancelled || !window.VLibras) return;
        container = mountContainer();
        new window.VLibras.Widget(APP_URL);
      })
      .catch((error) => console.error("[libras]", error));

    return () => {
      cancelled = true;
      container?.remove();
    };
  }, [libras]);

  return null;
}
