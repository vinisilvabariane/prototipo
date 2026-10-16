"use client";

import { useEffect, useLayoutEffect, useRef, useState } from "react";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { brand, easeOut, inkSoft, line } from "@/lib/tokens";
import { allModules, hubGroups } from "../modules";
import ModuleWindow from "./ModuleWindow";

type Props = {
  selected: string;
  matches: Set<string> | null;
  onSelect: (code: string) => void;
};

type Ring = { x: number; y: number; w: number; h: number };

const GROW = 4;
const order = new Map(allModules.map((module, index) => [module.code, index]));

export default function ModulePlate({ selected, matches, onSelect }: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [ring, setRing] = useState<Ring | null>(null);
  const [animate, setAnimate] = useState(false);

  useLayoutEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    function measure() {
      const tile = container?.querySelector<HTMLElement>(`[data-code="${selected}"]`);
      if (!container || !tile) return;
      const box = container.getBoundingClientRect();
      const rect = tile.getBoundingClientRect();
      setRing({ x: rect.left - box.left - GROW, y: rect.top - box.top - GROW, w: rect.width + GROW * 2, h: rect.height + GROW * 2 });
    }

    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(container);
    return () => observer.disconnect();
  }, [selected]);

  useEffect(() => {
    if (!ring || animate) return;
    const frame = requestAnimationFrame(() => setAnimate(true));
    return () => cancelAnimationFrame(frame);
  }, [ring, animate]);

  return (
    <Box ref={containerRef} sx={{ position: "relative" }}>
      {hubGroups.map((group, index) => (
        <Box
          key={group.id}
          component="section"
          aria-labelledby={`group-${group.id}`}
          sx={{
            display: "flex",
            flexDirection: { xs: "column", sm: "row" },
            gap: { xs: 1.5, sm: 3 },
            py: 2.5,
            borderTop: index === 0 ? 0 : `1px solid ${line}`,
            "&:first-of-type": { pt: 0 },
          }}
        >
          <Box sx={{ width: { sm: 132 }, flexShrink: 0 }}>
            <Typography id={`group-${group.id}`} variant="overline" component="h2" sx={{ display: "block", color: "text.primary" }}>
              {group.name}
            </Typography>
            <Typography variant="caption" sx={{ display: "block", mt: 0.5, color: inkSoft, lineHeight: 1.35 }}>
              {group.description}
            </Typography>
          </Box>
          <Box
            sx={{
              flex: 1,
              minWidth: 0,
              display: "grid",
              gap: 1.5,
              gridTemplateColumns: { xs: "repeat(2, minmax(0, 1fr))", sm: "repeat(3, minmax(0, 1fr))" },
            }}
          >
            {group.modules.map((module) => (
              <ModuleWindow
                key={module.code}
                module={module}
                index={order.get(module.code) ?? 0}
                selected={selected === module.code}
                dimmed={matches !== null && !matches.has(module.code)}
                onSelect={() => onSelect(module.code)}
              />
            ))}
          </Box>
        </Box>
      ))}

      <Box
        aria-hidden
        sx={{
          position: "absolute",
          left: 0,
          top: 0,
          width: ring?.w ?? 0,
          height: ring?.h ?? 0,
          transform: `translate(${ring?.x ?? 0}px, ${ring?.y ?? 0}px)`,
          border: `2px solid ${brand}`,
          borderRadius: "18px",
          pointerEvents: "none",
          opacity: ring ? 1 : 0,
          transition: animate
            ? `transform 0.55s ${easeOut}, width 0.55s ${easeOut}, height 0.55s ${easeOut}, opacity 0.3s ease`
            : "none",
          "@media (prefers-reduced-motion: reduce)": { transition: "none" },
        }}
      />
    </Box>
  );
}
