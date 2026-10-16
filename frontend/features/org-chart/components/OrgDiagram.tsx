"use client";

import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import Typography from "@mui/material/Typography";
import { brand, brandSoft, ink, inkSoft, line, lineStrong } from "@/lib/tokens";
import { countBelow } from "../tree";
import type { OrgNode } from "../types";

export type TreeContext = {
  selectedId: number | null;
  collapsed: Set<number>;
  forcedOpen: Set<number>;
  matched: Set<number> | null;
  onSelect: (id: number) => void;
  onToggle: (id: number) => void;
};

const CONNECTOR = 22;

const connectors = {
  "& ul": {
    display: "flex",
    justifyContent: "center",
    listStyle: "none",
    m: 0,
    p: 0,
    pt: `${CONNECTOR}px`,
    position: "relative",
  },
  "& ul ul::before": {
    content: '""',
    position: "absolute",
    top: 0,
    left: "50%",
    height: `${CONNECTOR}px`,
    borderLeft: `1px solid ${lineStrong}`,
  },
  "& li": {
    position: "relative",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    px: "8px",
    pt: `${CONNECTOR}px`,
  },
  "& li::before, & li::after": {
    content: '""',
    position: "absolute",
    top: 0,
    right: "50%",
    width: "50%",
    height: `${CONNECTOR}px`,
    borderTop: `1px solid ${lineStrong}`,
  },
  "& li::after": { right: "auto", left: "50%", borderLeft: `1px solid ${lineStrong}` },
  "& li:only-child": { pt: 0 },
  "& li:only-child::before, & li:only-child::after": { display: "none" },
  "& li:first-of-type::before, & li:last-of-type::after": { border: 0 },
  "& li:last-of-type::before": { borderRight: `1px solid ${lineStrong}`, borderRadius: "0 8px 0 0" },
  "& li:first-of-type::after": { borderRadius: "8px 0 0 0" },
} as const;

function Card({ node, ctx }: { node: OrgNode; ctx: TreeContext }) {
  const { position } = node;
  const selected = ctx.selectedId === position.id;
  const dimmed = ctx.matched !== null && !ctx.matched.has(position.id);
  const highlighted = ctx.matched !== null && ctx.matched.has(position.id);
  const vacant = position.holder === null;

  return (
    <ButtonBase
      onClick={() => ctx.onSelect(position.id)}
      aria-pressed={selected}
      sx={{
        width: 196,
        minHeight: 78,
        p: 1.5,
        display: "flex",
        flexDirection: "column",
        alignItems: "flex-start",
        gap: 0.25,
        textAlign: "left",
        borderRadius: "12px",
        bgcolor: highlighted ? brandSoft : "background.paper",
        border: vacant ? `1px dashed ${lineStrong}` : `1px solid ${selected ? brand : line}`,
        outline: selected ? `2px solid ${brand}` : "none",
        outlineOffset: 1,
        opacity: dimmed ? 0.4 : 1,
        transition: "opacity 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease",
        "&:hover": { borderColor: brand },
        "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2 },
      }}
    >
      <Typography sx={{ fontWeight: 600, fontSize: "0.85rem", lineHeight: 1.25, color: ink, width: "100%" }}>{position.title}</Typography>
      <Typography
        sx={{
          fontSize: "0.8rem",
          lineHeight: 1.3,
          width: "100%",
          color: vacant ? inkSoft : ink,
          fontStyle: vacant ? "italic" : "normal",
          overflow: "hidden",
          textOverflow: "ellipsis",
          whiteSpace: "nowrap",
        }}
      >
        {position.holder ? position.holder.name : "Vago"}
        {position.holder && !position.holder.active ? " (inativo)" : ""}
      </Typography>
      <Typography sx={{ fontSize: "0.66rem", fontWeight: 600, letterSpacing: "0.1em", textTransform: "uppercase", color: inkSoft, mt: 0.25 }}>
        {position.department}
      </Typography>
    </ButtonBase>
  );
}

function Branch({ node, ctx }: { node: OrgNode; ctx: TreeContext }) {
  const { position } = node;
  const hasChildren = node.children.length > 0;
  const collapsed = hasChildren && ctx.collapsed.has(position.id) && !ctx.forcedOpen.has(position.id);

  return (
    <li>
      <Card node={node} ctx={ctx} />
      {hasChildren && (
        <ButtonBase
          onClick={() => ctx.onToggle(position.id)}
          aria-expanded={!collapsed}
          aria-label={collapsed ? `Expandir subordinados de ${position.title}` : `Recolher subordinados de ${position.title}`}
          sx={{
            mt: 0.75,
            minWidth: 26,
            height: 22,
            px: 0.75,
            borderRadius: "11px",
            border: `1px solid ${lineStrong}`,
            bgcolor: "background.paper",
            fontSize: "0.7rem",
            fontWeight: 600,
            color: inkSoft,
            fontVariantNumeric: "tabular-nums",
            position: "relative",
            zIndex: 1,
            "&:hover": { borderColor: brand, color: ink },
            "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2 },
          }}
        >
          {collapsed ? `+${countBelow(node)}` : "−"}
        </ButtonBase>
      )}
      {hasChildren && !collapsed && (
        <ul>
          {node.children.map((child) => (
            <Branch key={child.position.id} node={child} ctx={ctx} />
          ))}
        </ul>
      )}
    </li>
  );
}

export default function OrgDiagram({ roots, ctx, zoom }: { roots: OrgNode[]; ctx: TreeContext; zoom: number }) {
  return (
    <Box
      sx={{
        overflow: "auto",
        border: `1px solid ${line}`,
        borderRadius: "16px",
        bgcolor: "background.paper",
        p: 3,
        minHeight: 360,
        maxHeight: "70vh",
      }}
    >
      <Box sx={{ ...connectors, "& > div > ul": { pt: 0 }, zoom, width: "max-content", minWidth: "100%" }}>
        <Box sx={{ display: "flex", justifyContent: "center", alignItems: "flex-start", gap: 4 }}>
          {roots.map((root) => (
            <ul key={root.position.id}>
              <Branch node={root} ctx={ctx} />
            </ul>
          ))}
        </Box>
      </Box>
    </Box>
  );
}
