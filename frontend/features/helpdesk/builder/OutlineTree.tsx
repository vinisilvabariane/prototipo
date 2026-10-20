"use client";

import { useState, type MouseEvent } from "react";
import AddOutlined from "@mui/icons-material/AddOutlined";
import SubdirectoryArrowRight from "@mui/icons-material/SubdirectoryArrowRight";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import ButtonBase from "@mui/material/ButtonBase";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { brand, brandSoft, inkSoft, line } from "@/lib/tokens";
import { buildTree, FIELD_HINTS, FIELD_LABELS, FIELD_TYPES, isChoice, isSpecial, type TreeNode } from "../schema";
import type { FieldType, FormField } from "../types";

type Props = {
  fields: FormField[];
  selectedId: string | null;
  blocked: FieldType[];
  onSelect: (id: string) => void;
  onAddRoot: (type: FieldType) => void;
  onAddChild: (parentId: string, optionValue: string, type: FieldType) => void;
};

type MenuState = { anchor: HTMLElement; parentId: string | null; optionValue: string | null } | null;

function TypeMark({ type }: { type: FieldType }) {
  return (
    <Box
      aria-hidden
      sx={{ minWidth: 30, height: 22, px: 0.5, borderRadius: "5px", display: "grid", placeItems: "center", fontSize: 10, fontWeight: 800, letterSpacing: 0.3, color: isSpecial(type) ? "#fff" : inkSoft, bgcolor: isSpecial(type) ? brand : "transparent", border: `1px solid ${isSpecial(type) ? brand : line}` }}
    >
      {{ text: "ABC", textArea: "¶", number: "123", select: "▾", cardRadio: "▦", productionSpot: "LIN", lineStop: "PAR", defects: "DEF" }[type]}
    </Box>
  );
}

function Node({ node, depth, selectedId, onSelect, openMenu }: { node: TreeNode; depth: number; selectedId: string | null; onSelect: (id: string) => void; openMenu: (event: MouseEvent<HTMLElement>, parentId: string, optionValue: string) => void }) {
  const selected = node.field.id === selectedId;

  return (
    <Box>
      <ButtonBase
        onClick={() => onSelect(node.field.id)}
        aria-current={selected}
        sx={{
          width: "100%",
          textAlign: "left",
          display: "flex",
          alignItems: "center",
          gap: 1,
          pl: 1.25 + depth * 0.5,
          pr: 1,
          py: 0.9,
          borderRadius: 1,
          border: `1px solid ${selected ? brand : "transparent"}`,
          bgcolor: selected ? brandSoft : "transparent",
          "&:hover": { bgcolor: brandSoft },
        }}
      >
        <TypeMark type={node.field.type} />
        <Typography variant="body2" noWrap sx={{ flex: 1, minWidth: 0, fontWeight: selected ? 700 : 600 }}>
          {node.field.label || "Sem título"}
        </Typography>
        {node.field.required && (
          <Typography component="span" aria-label="obrigatório" sx={{ color: brand, fontWeight: 700 }}>
            *
          </Typography>
        )}
      </ButtonBase>

      {isChoice(node.field.type) &&
        node.branches.map((branch) => (
          <Box key={branch.option.value} sx={{ ml: 2.25 + depth * 0.5, pl: 1.25, borderLeft: `2px solid ${line}`, mt: 0.25 }}>
            <Stack direction="row" spacing={0.75} sx={{ alignItems: "center", py: 0.5 }}>
              <SubdirectoryArrowRight sx={{ fontSize: 15, color: inkSoft }} />
              <Typography variant="caption" sx={{ color: branch.orphan ? "warning.main" : inkSoft, fontWeight: 700, flex: 1, minWidth: 0 }} noWrap>
                Se for «{branch.option.label || branch.option.value}»
              </Typography>
              {!branch.orphan && (
                <Button size="small" sx={{ minWidth: 0, px: 0.75, py: 0, fontSize: 12 }} startIcon={<AddOutlined sx={{ fontSize: 14 }} />} onClick={(event) => openMenu(event, node.field.id, branch.option.value)}>
                  Pergunta
                </Button>
              )}
            </Stack>
            {branch.children.map((child) => (
              <Node key={child.field.id} node={child} depth={depth + 1} selectedId={selectedId} onSelect={onSelect} openMenu={openMenu} />
            ))}
          </Box>
        ))}
    </Box>
  );
}

export default function OutlineTree({ fields, selectedId, blocked, onSelect, onAddRoot, onAddChild }: Props) {
  const [menu, setMenu] = useState<MenuState>(null);
  const tree = buildTree(fields);
  const used = new Set(fields.filter((field) => isSpecial(field.type)).map((field) => field.type));

  function open(event: MouseEvent<HTMLElement>, parentId: string | null, optionValue: string | null) {
    setMenu({ anchor: event.currentTarget, parentId, optionValue });
  }

  function pick(type: FieldType) {
    if (menu?.parentId && menu.optionValue !== null) onAddChild(menu.parentId, menu.optionValue, type);
    else onAddRoot(type);
    setMenu(null);
  }

  return (
    <Box>
      <Stack spacing={0.25}>
        {tree.map((node) => (
          <Node key={node.field.id} node={node} depth={0} selectedId={selectedId} onSelect={onSelect} openMenu={open} />
        ))}
      </Stack>
      {fields.length === 0 && (
        <Typography variant="body2" sx={{ color: inkSoft, py: 2 }}>
          Nenhuma pergunta ainda. Comece adicionando a primeira.
        </Typography>
      )}
      <Button fullWidth variant="outlined" startIcon={<AddOutlined />} sx={{ mt: 1.5 }} onClick={(event) => open(event, null, null)}>
        Adicionar pergunta
      </Button>
      <Menu anchorEl={menu?.anchor} open={menu !== null} onClose={() => setMenu(null)}>
        {FIELD_TYPES.filter((type) => !blocked.includes(type)).map((type) => (
          <MenuItem key={type} disabled={isSpecial(type) && used.has(type)} onClick={() => pick(type)} sx={{ display: "block", maxWidth: 320, whiteSpace: "normal" }}>
            <Typography variant="body2" sx={{ fontWeight: 700 }}>
              {FIELD_LABELS[type]}
            </Typography>
            <Typography variant="caption" sx={{ color: inkSoft }}>
              {isSpecial(type) && used.has(type) ? "Já está no formulário." : FIELD_HINTS[type]}
            </Typography>
          </MenuItem>
        ))}
      </Menu>
    </Box>
  );
}
