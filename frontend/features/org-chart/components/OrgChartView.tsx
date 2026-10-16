"use client";

import { useMemo, useState } from "react";
import Add from "@mui/icons-material/Add";
import Remove from "@mui/icons-material/Remove";
import Search from "@mui/icons-material/Search";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import Typography from "@mui/material/Typography";
import FormDialog from "@/components/ui/FormDialog";
import type { Department } from "@/features/departments/types";
import { inkSoft } from "@/lib/tokens";
import { createPosition } from "../actions/orgChartActions";
import { ancestorsOf, buildTree, indexById, isFiltering, matchingIds } from "../tree";
import type { OrgChart, OrgUser } from "../types";
import OrgDiagram, { type TreeContext } from "./OrgDiagram";
import OrgOutline from "./OrgOutline";
import PositionFields from "./PositionFields";
import PositionPanel from "./PositionPanel";

type View = "diagram" | "list";

const ZOOMS = [0.6, 0.75, 0.9, 1, 1.15, 1.3];

type Props = { chart: OrgChart; departments: Department[]; users: OrgUser[] };

export default function OrgChartView({ chart, departments, users }: Props) {
  const { positions, canManage } = chart;

  const [view, setView] = useState<View>("diagram");
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [collapsed, setCollapsed] = useState<Set<number>>(new Set());
  const [query, setQuery] = useState("");
  const [departmentId, setDepartmentId] = useState<number | null>(null);
  const [zoomIndex, setZoomIndex] = useState(3);

  const byId = useMemo(() => indexById(positions), [positions]);
  const roots = useMemo(() => buildTree(positions), [positions]);

  const filter = useMemo(() => ({ query, departmentId }), [query, departmentId]);
  const filtering = isFiltering(filter);
  const matched = useMemo(() => (filtering ? matchingIds(positions, filter) : null), [filtering, positions, filter]);
  const forcedOpen = useMemo(() => (matched ? ancestorsOf(byId, matched) : new Set<number>()), [matched, byId]);

  const selected = selectedId === null ? undefined : byId.get(selectedId);
  const vacant = positions.filter((position) => position.holder === null).length;
  const withChildren = useMemo(() => new Set(positions.filter((position) => position.parentId !== null).map((position) => position.parentId!)), [positions]);

  const ctx: TreeContext = {
    selectedId,
    collapsed,
    forcedOpen,
    matched,
    onSelect: setSelectedId,
    onToggle: (id) =>
      setCollapsed((current) => {
        const next = new Set(current);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        return next;
      }),
  };

  const create = (
    <FormDialog title="Novo cargo de topo" trigger="Novo cargo de topo" submitLabel="Criar cargo" action={createPosition.bind(null, null)}>
      <PositionFields departments={departments} holders={users} />
    </FormDialog>
  );

  if (positions.length === 0) {
    return (
      <Stack spacing={2} sx={{ alignItems: "flex-start" }}>
        <Typography sx={{ color: inkSoft }}>
          O organograma ainda não tem cargos.{canManage ? " Comece criando o cargo de topo da estrutura." : " Peça a quem mantém o organograma para cadastrá-los."}
        </Typography>
        {canManage && create}
      </Stack>
    );
  }

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: "column", lg: "row" }} spacing={2} sx={{ justifyContent: "space-between", alignItems: { lg: "center" } }}>
        <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5}>
          <TextField
            size="small"
            label="Buscar cargo ou pessoa"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            sx={{ minWidth: { sm: 260 } }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <Search fontSize="small" />
                  </InputAdornment>
                ),
              },
            }}
          />
          <TextField
            select
            size="small"
            label="Departamento"
            value={departmentId ?? ""}
            onChange={(e) => setDepartmentId(e.target.value === "" ? null : Number(e.target.value))}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">Todos</MenuItem>
            {departments.map((department) => (
              <MenuItem key={department.id} value={department.id}>
                {department.name}
              </MenuItem>
            ))}
          </TextField>
        </Stack>

        <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
          <Button size="small" onClick={() => setCollapsed(new Set())}>
            Expandir tudo
          </Button>
          <Button size="small" onClick={() => setCollapsed(new Set(withChildren))}>
            Recolher tudo
          </Button>
          {view === "diagram" && (
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <IconButton size="small" aria-label="Diminuir zoom" disabled={zoomIndex === 0} onClick={() => setZoomIndex((value) => value - 1)}>
                <Remove fontSize="small" />
              </IconButton>
              <Typography variant="body2" sx={{ minWidth: 42, textAlign: "center", fontVariantNumeric: "tabular-nums" }}>
                {Math.round(ZOOMS[zoomIndex] * 100)}%
              </Typography>
              <IconButton size="small" aria-label="Aumentar zoom" disabled={zoomIndex === ZOOMS.length - 1} onClick={() => setZoomIndex((value) => value + 1)}>
                <Add fontSize="small" />
              </IconButton>
            </Stack>
          )}
          <ToggleButtonGroup size="small" exclusive value={view} onChange={(_, next: View | null) => next && setView(next)} aria-label="Modo de exibição">
            <ToggleButton value="diagram">Diagrama</ToggleButton>
            <ToggleButton value="list">Lista</ToggleButton>
          </ToggleButtonGroup>
          {canManage && create}
        </Stack>
      </Stack>

      <Typography variant="body2" sx={{ color: inkSoft }} aria-live="polite">
        {positions.length} cargos · {positions.length - vacant} ocupados · {vacant} vagos
        {matched ? ` · ${matched.size} ${matched.size === 1 ? "resultado" : "resultados"} na busca` : ""}
      </Typography>

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "minmax(0, 1fr) 360px" }, alignItems: "start" }}>
        {view === "diagram" ? <OrgDiagram roots={roots} ctx={ctx} zoom={ZOOMS[zoomIndex]} /> : <OrgOutline roots={roots} ctx={ctx} />}

        <Box sx={{ position: { lg: "sticky" }, top: { lg: 88 } }}>
          {selected ? (
            <PositionPanel
              key={selected.id}
              position={selected}
              positions={positions}
              departments={departments}
              users={users}
              canManage={canManage}
              onSelect={setSelectedId}
            />
          ) : (
            <Box sx={{ p: 3, border: "1px dashed", borderColor: "divider", borderRadius: "16px" }}>
              <Typography sx={{ color: inkSoft }}>Selecione um cargo para ver o ocupante, a cadeia de comando e a equipe.</Typography>
            </Box>
          )}
        </Box>
      </Box>
    </Stack>
  );
}
