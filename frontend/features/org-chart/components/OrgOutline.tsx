"use client";

import ChevronRight from "@mui/icons-material/ChevronRight";
import ExpandMore from "@mui/icons-material/ExpandMore";
import Box from "@mui/material/Box";
import ButtonBase from "@mui/material/ButtonBase";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import { brand, brandSoft, inkSoft, line } from "@/lib/tokens";
import { countBelow } from "../tree";
import type { OrgNode } from "../types";
import type { TreeContext } from "./OrgDiagram";

function Row({ node, ctx }: { node: OrgNode; ctx: TreeContext }) {
  const { position } = node;
  const hasChildren = node.children.length > 0;
  const collapsed = hasChildren && ctx.collapsed.has(position.id) && !ctx.forcedOpen.has(position.id);
  const selected = ctx.selectedId === position.id;
  const dimmed = ctx.matched !== null && !ctx.matched.has(position.id);
  const highlighted = ctx.matched !== null && ctx.matched.has(position.id);

  return (
    <Box component="li" sx={{ listStyle: "none" }}>
      <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, opacity: dimmed ? 0.45 : 1 }}>
        {hasChildren ? (
          <IconButton
            size="small"
            onClick={() => ctx.onToggle(position.id)}
            aria-expanded={!collapsed}
            aria-label={collapsed ? `Expandir subordinados de ${position.title}` : `Recolher subordinados de ${position.title}`}
          >
            {collapsed ? <ChevronRight fontSize="small" /> : <ExpandMore fontSize="small" />}
          </IconButton>
        ) : (
          <Box sx={{ width: 34, flexShrink: 0 }} />
        )}
        <ButtonBase
          onClick={() => ctx.onSelect(position.id)}
          aria-pressed={selected}
          sx={{
            flex: 1,
            minWidth: 0,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            gap: 2,
            textAlign: "left",
            px: 1.5,
            py: 1,
            borderRadius: "10px",
            bgcolor: highlighted ? brandSoft : "transparent",
            border: `1px solid ${selected ? brand : "transparent"}`,
            "&:hover": { bgcolor: brandSoft },
            "&:focus-visible": { outline: `2px solid ${brand}`, outlineOffset: 2 },
          }}
        >
          <Box sx={{ minWidth: 0 }}>
            <Typography sx={{ fontWeight: 600, fontSize: "0.9rem" }}>{position.title}</Typography>
            <Typography variant="body2" sx={{ color: inkSoft, fontStyle: position.holder ? "normal" : "italic" }}>
              {position.holder ? position.holder.name : "Vago"} · {position.department}
            </Typography>
          </Box>
          {collapsed && (
            <Typography variant="body2" sx={{ color: inkSoft, flexShrink: 0, fontVariantNumeric: "tabular-nums" }}>
              +{countBelow(node)}
            </Typography>
          )}
        </ButtonBase>
      </Box>

      {hasChildren && !collapsed && (
        <Box component="ul" sx={{ m: 0, ml: 2.5, pl: 1.5, borderLeft: `1px solid ${line}` }}>
          {node.children.map((child) => (
            <Row key={child.position.id} node={child} ctx={ctx} />
          ))}
        </Box>
      )}
    </Box>
  );
}

export default function OrgOutline({ roots, ctx }: { roots: OrgNode[]; ctx: TreeContext }) {
  return (
    <Box sx={{ border: `1px solid ${line}`, borderRadius: "16px", bgcolor: "background.paper", p: 2, minHeight: 240 }}>
      <Box component="ul" sx={{ m: 0, p: 0 }}>
        {roots.map((root) => (
          <Row key={root.position.id} node={root} ctx={ctx} />
        ))}
      </Box>
    </Box>
  );
}
