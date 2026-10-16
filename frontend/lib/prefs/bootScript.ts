import { COLOR_VISIONS, CONTRASTS, FONT_SIZES, LINE_HEIGHTS, MOTIONS, PREFS_STORAGE_KEY, THEME_NAMES } from "./options";

const fields = [
  ["theme", THEME_NAMES],
  ["fontSize", FONT_SIZES],
  ["lineHeight", LINE_HEIGHTS],
  ["motion", MOTIONS],
  ["contrast", CONTRASTS],
  ["colorVision", COLOR_VISIONS],
] as const;

export const prefsBootScript = `(function(){try{var p=JSON.parse(localStorage.getItem(${JSON.stringify(PREFS_STORAGE_KEY)})||"{}");var d=document.documentElement.dataset;${fields
  .map(([key, options]) => `if(${JSON.stringify(options)}.indexOf(p.${key})>=0)d.${key}=p.${key};`)
  .join("")}}catch(e){}})();`;
