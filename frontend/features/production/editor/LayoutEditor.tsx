"use client";

import { useCallback, useEffect, useMemo, useRef, useState, useTransition } from "react";
import { applyNodeChanges, type NodeChange, type ReactFlowInstance } from "@xyflow/react";
import Add from "@mui/icons-material/Add";
import GridView from "@mui/icons-material/GridView";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import ListItemText from "@mui/material/ListItemText";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import Snackbar from "@mui/material/Snackbar";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import { saveLayout } from "../actions/layoutActions";
import CanvasView from "../canvas/CanvasView";
import { arrangeInGrid, findFreeSpot, POST_HEIGHT, POST_WIDTH, snap } from "../canvas/geometry";
import {
  decorationNodeId,
  isDecorationNode,
  isPostNode,
  modelId,
  postNodeId,
  type CanvasNode,
  type DecorationNodeData,
  type DecorationNodeType,
  type PostNodeData,
  type PostNodeType,
} from "../canvas/nodeData";
import { DECORATION_DEFAULTS, DECORATION_KINDS, DECORATION_LABELS, POST_KINDS, POST_KIND_HINTS, POST_KIND_LABELS } from "../labels";
import type { DecorationKind, LayoutDecoration, LayoutDetail, LayoutPost, PostKind, ProductionLine, ProductionProduct, ProductionShift, SaveLayoutBody } from "../types";
import CopyLayoutDialog from "./CopyLayoutDialog";
import Inspector from "./Inspector";

type Props = {
  layout: LayoutDetail;
  lines: ProductionLine[];
  products: ProductionProduct[];
  shifts: ProductionShift[];
};

const KIND_NAME_PREFIX: Record<PostKind, string> = { standard: "Posto", machine: "Máquina", quality: "Qualidade" };

function postNode(post: LayoutPost, editable: boolean): PostNodeType {
  return {
    id: postNodeId(post.id),
    type: "post",
    position: { x: post.x, y: post.y },
    width: post.width,
    height: post.height,
    zIndex: 1,
    data: {
      kind: post.kind,
      name: post.name,
      isCritical: post.isCritical,
      targetPerHour: post.targetPerHour,
      operation: post.operation,
      rotation: post.rotation,
      editable,
      holder: null,
      showMetrics: false,
      produced: 0,
      expected: null,
      attainment: null,
      invalid: false,
    },
  };
}

function decorationNode(decoration: LayoutDecoration, editable: boolean): DecorationNodeType {
  return {
    id: decorationNodeId(decoration.id),
    type: "decoration",
    position: { x: decoration.x, y: decoration.y },
    width: decoration.width,
    height: decoration.height,
    zIndex: 0,
    draggable: !decoration.locked,
    data: { kind: decoration.kind, label: decoration.label, rotation: decoration.rotation, locked: decoration.locked, editable },
  };
}

function toNodes(layout: LayoutDetail, editable: boolean): CanvasNode[] {
  return [...layout.decorations.map((item) => decorationNode(item, editable)), ...layout.posts.map((item) => postNode(item, editable))];
}

const sizeOf = (node: CanvasNode, fallback: { width: number; height: number }) => ({
  width: Math.round(node.width ?? node.measured?.width ?? fallback.width),
  height: Math.round(node.height ?? node.measured?.height ?? fallback.height),
});

function snapshotOf(nodes: CanvasNode[]): SaveLayoutBody {
  return {
    posts: nodes.filter(isPostNode).map((node) => ({
      id: modelId(node),
      name: node.data.name.trim(),
      kind: node.data.kind,
      x: Math.round(node.position.x),
      y: Math.round(node.position.y),
      ...sizeOf(node, { width: POST_WIDTH, height: POST_HEIGHT }),
      rotation: node.data.rotation,
      isCritical: node.data.isCritical,
      targetPerHour: node.data.targetPerHour,
      operation: node.data.operation?.trim() || null,
    })),
    decorations: nodes.filter(isDecorationNode).map((node) => ({
      id: modelId(node),
      kind: node.data.kind,
      x: Math.round(node.position.x),
      y: Math.round(node.position.y),
      ...sizeOf(node, { width: 120, height: 60 }),
      rotation: node.data.rotation,
      label: node.data.label?.trim() || null,
      locked: node.data.locked,
    })),
  };
}

function problemsOf(nodes: CanvasNode[]) {
  const problems = new Map<string, string>();
  const seen = new Map<string, string>();

  for (const node of nodes.filter(isPostNode)) {
    const name = node.data.name.trim().toLowerCase();

    if (!name) {
      problems.set(node.id, "Informe o nome do posto.");
      continue;
    }

    const first = seen.get(name);
    if (first) {
      problems.set(node.id, "Já existe um posto com esse nome.");
      problems.set(first, "Já existe um posto com esse nome.");
    } else {
      seen.set(name, node.id);
    }
  }

  return problems;
}

function uniqueName(base: string, taken: Set<string>) {
  if (!taken.has(base.toLowerCase())) return base;

  for (let index = 2; ; index++) {
    const candidate = `${base} ${index}`;
    if (!taken.has(candidate.toLowerCase())) return candidate;
  }
}

const isTyping = (target: EventTarget | null) =>
  target instanceof HTMLElement && (["INPUT", "TEXTAREA", "SELECT"].includes(target.tagName) || target.isContentEditable);

export default function LayoutEditor({ layout, lines, products, shifts }: Props) {
  const editable = layout.canManage;
  const [nodes, setNodes] = useState<CanvasNode[]>(() => toNodes(layout, editable));
  const [baseline, setBaseline] = useState(() => JSON.stringify(snapshotOf(toNodes(layout, editable))));
  const [error, setError] = useState<AppError>();
  const [savedAt, setSavedAt] = useState(false);
  const [discarding, setDiscarding] = useState(false);
  const [postMenu, setPostMenu] = useState<HTMLElement | null>(null);
  const [decorationMenu, setDecorationMenu] = useState<HTMLElement | null>(null);
  const [pending, startTransition] = useTransition();
  const instance = useRef<ReactFlowInstance<CanvasNode> | null>(null);
  const tempId = useRef(0);

  const problems = useMemo(() => problemsOf(nodes), [nodes]);
  const dirty = useMemo(() => JSON.stringify(snapshotOf(nodes)) !== baseline, [nodes, baseline]);
  const selected = useMemo(() => nodes.filter((node) => node.selected), [nodes]);

  const displayNodes = useMemo(
    () => nodes.map((node) => (isPostNode(node) && problems.has(node.id) ? { ...node, data: { ...node.data, invalid: true } } : node)),
    [nodes, problems],
  );

  const nextTempId = () => --tempId.current;

  const onNodesChange = useCallback((changes: NodeChange<CanvasNode>[]) => {
    setNodes((current) => applyNodeChanges(changes, current));
  }, []);

  const patchPost = useCallback((id: string, patch: Partial<PostNodeData>) => {
    setNodes((current) => current.map((node) => (node.id === id && isPostNode(node) ? { ...node, data: { ...node.data, ...patch } } : node)));
  }, []);

  const patchDecoration = useCallback((id: string, patch: Partial<DecorationNodeData>) => {
    setNodes((current) =>
      current.map((node) =>
        node.id === id && isDecorationNode(node) ? { ...node, draggable: patch.locked === undefined ? node.draggable : !patch.locked, data: { ...node.data, ...patch } } : node,
      ),
    );
  }, []);

  const resize = useCallback((id: string, width: number, height: number) => {
    setNodes((current) => current.map((node) => (node.id === id ? { ...node, width, height } : node)));
  }, []);

  const focusOn = (node: CanvasNode) => {
    const size = sizeOf(node, { width: POST_WIDTH, height: POST_HEIGHT });
    instance.current?.setCenter(node.position.x + size.width / 2, node.position.y + size.height / 2, { zoom: Math.max(instance.current.getZoom(), 0.6), duration: 250 });
  };

  const placeNew = (node: CanvasNode) => {
    setNodes((current) => [...current.map((item) => ({ ...item, selected: false })), { ...node, selected: true }]);
    focusOn(node);
  };

  const boxesOf = (list: CanvasNode[]) => list.map((node) => ({ ...node.position, ...sizeOf(node, { width: POST_WIDTH, height: POST_HEIGHT }) }));

  const addPost = (kind: PostKind) => {
    setPostMenu(null);
    const taken = new Set(nodes.filter(isPostNode).map((node) => node.data.name.trim().toLowerCase()));
    const count = nodes.filter((node) => isPostNode(node) && node.data.kind === kind).length + 1;
    const id = nextTempId();

    placeNew({
      id: postNodeId(id),
      type: "post",
      position: findFreeSpot(boxesOf(nodes.filter(isPostNode)), POST_WIDTH, POST_HEIGHT),
      width: POST_WIDTH,
      height: POST_HEIGHT,
      zIndex: 1,
      data: {
        kind,
        name: uniqueName(`${KIND_NAME_PREFIX[kind]} ${count}`, taken),
        isCritical: false,
        targetPerHour: null,
        operation: null,
        rotation: 0,
        editable: true,
        holder: null,
        showMetrics: false,
        produced: 0,
        expected: null,
        attainment: null,
        invalid: false,
      },
    });
  };

  const addDecoration = (kind: DecorationKind) => {
    setDecorationMenu(null);
    const defaults = DECORATION_DEFAULTS[kind];
    const id = nextTempId();

    placeNew({
      id: decorationNodeId(id),
      type: "decoration",
      position: findFreeSpot(boxesOf(nodes.filter(isDecorationNode)), defaults.width, defaults.height),
      width: defaults.width,
      height: defaults.height,
      zIndex: 0,
      data: { kind, label: defaults.label, rotation: 0, locked: false, editable: true },
    });
  };

  const deleteSelected = useCallback(() => {
    setNodes((current) => current.filter((node) => !node.selected));
  }, []);

  const duplicateSelected = useCallback(() => {
    setNodes((current) => {
      const chosen = current.filter((node) => node.selected);
      if (chosen.length === 0) return current;

      const taken = new Set(current.filter(isPostNode).map((node) => node.data.name.trim().toLowerCase()));
      const occupied = current.map((node) => ({ ...node.position, ...sizeOf(node, { width: POST_WIDTH, height: POST_HEIGHT }) }));
      const copies: CanvasNode[] = [];

      for (const node of chosen) {
        const size = sizeOf(node, { width: POST_WIDTH, height: POST_HEIGHT });
        const position = findFreeSpot(occupied, size.width, size.height);
        occupied.push({ ...position, ...size });
        const id = --tempId.current;

        if (isPostNode(node)) {
          const name = uniqueName(`${node.data.name.trim() || "Posto"} (cópia)`, taken);
          taken.add(name.toLowerCase());
          copies.push({ ...node, id: postNodeId(id), position, selected: true, data: { ...node.data, name } });
        } else if (isDecorationNode(node)) {
          copies.push({ ...node, id: decorationNodeId(id), position, selected: true });
        }
      }

      return [...current.map((node) => ({ ...node, selected: false })), ...copies];
    });
  }, []);

  const arrange = () => {
    const posts = nodes.filter(isPostNode).sort((a, b) => a.position.y - b.position.y || a.position.x - b.position.x);
    const spots = arrangeInGrid(boxesOf(posts));
    const moved = new Map(posts.map((node, index) => [node.id, spots[index]]));

    setNodes((current) => current.map((node) => (moved.has(node.id) ? { ...node, position: moved.get(node.id)! } : node)));
    requestAnimationFrame(() => instance.current?.fitView({ duration: 250, padding: 0.12 }));
  };

  const discard = () => {
    setNodes(toNodes(layout, editable));
    setBaseline(JSON.stringify(snapshotOf(toNodes(layout, editable))));
    setDiscarding(false);
    setError(undefined);
  };

  const save = useCallback(() => {
    if (!editable || problems.size > 0 || pending) return;
    setError(undefined);

    startTransition(async () => {
      const result = await saveLayout(layout.id, snapshotOf(nodes));

      if (!result.ok) {
        setError(result.error);
        return;
      }

      const fresh = toNodes(result.value, editable);
      setNodes(fresh);
      setBaseline(JSON.stringify(snapshotOf(fresh)));
      setSavedAt(true);
    });
  }, [editable, problems, pending, layout.id, nodes]);

  useEffect(() => {
    if (!editable) return;

    const onKey = (event: KeyboardEvent) => {
      if (isTyping(event.target)) return;

      if ((event.key === "Delete" || event.key === "Backspace") && nodes.some((node) => node.selected)) {
        event.preventDefault();
        deleteSelected();
      } else if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "s") {
        event.preventDefault();
        save();
      } else if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "d") {
        event.preventDefault();
        duplicateSelected();
      }
    };

    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [editable, nodes, deleteSelected, duplicateSelected, save]);

  useEffect(() => {
    if (!dirty) return;

    const warn = (event: BeforeUnloadEvent) => event.preventDefault();
    window.addEventListener("beforeunload", warn);
    return () => window.removeEventListener("beforeunload", warn);
  }, [dirty]);

  const postCount = nodes.filter(isPostNode).length;

  return (
    <Stack spacing={2}>
      {editable ? (
        <Stack direction={{ xs: "column", md: "row" }} spacing={1.5} sx={{ justifyContent: "space-between", alignItems: { md: "center" } }}>
          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", rowGap: 1 }}>
            <Button variant="contained" startIcon={<Add />} onClick={(event) => setPostMenu(event.currentTarget)}>
              Posto
            </Button>
            <Button variant="outlined" startIcon={<Add />} onClick={(event) => setDecorationMenu(event.currentTarget)}>
              Decoração
            </Button>
            <Button variant="outlined" startIcon={<GridView />} onClick={arrange} disabled={postCount === 0}>
              Organizar em grade
            </Button>
          </Stack>
          <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
            <Typography variant="body2" sx={{ color: dirty ? "var(--mui-palette-warning-main)" : inkSoft, fontWeight: dirty ? 600 : 400 }}>
              {dirty ? "Alterações não salvas" : "Tudo salvo"}
            </Typography>
            {dirty && <Button onClick={() => setDiscarding(true)}>Descartar</Button>}
            <CopyLayoutDialog
              layoutId={layout.id}
              origin={{ lineId: layout.lineId, productId: layout.productId, shiftId: layout.shiftId }}
              lines={lines}
              products={products}
              shifts={shifts}
              blockedReason={dirty ? "Salve o layout antes de copiar." : null}
            />
            <Button variant="contained" onClick={save} disabled={!dirty || pending || problems.size > 0}>
              {pending ? "Salvando..." : "Salvar layout"}
            </Button>
          </Stack>
        </Stack>
      ) : (
        <Alert severity="info">Você pode consultar este layout, mas não editá-lo.</Alert>
      )}

      {error && <ErrorAlert error={error} />}
      {problems.size > 0 && editable && <Alert severity="warning">Corrija os nomes dos postos destacados em vermelho para salvar.</Alert>}

      <Box sx={{ display: "grid", gap: 2, gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "minmax(0, 1fr) 320px" }, alignItems: "start" }}>
        <CanvasView nodes={displayNodes} editable={editable} onNodesChange={onNodesChange} onInit={(created) => { instance.current = created; }} />
        <Card sx={{ p: 2.5, position: { lg: "sticky" }, top: { lg: 80 } }}>
          <Inspector
            selected={selected}
            problems={problems}
            onPatchPost={patchPost}
            onPatchDecoration={patchDecoration}
            onResize={(id, width, height) => resize(id, snap(width), snap(height))}
            onDuplicate={duplicateSelected}
            onDelete={deleteSelected}
          />
        </Card>
      </Box>

      <Menu anchorEl={postMenu} open={Boolean(postMenu)} onClose={() => setPostMenu(null)}>
        {POST_KINDS.map((kind) => (
          <MenuItem key={kind} onClick={() => addPost(kind)}>
            <ListItemText primary={POST_KIND_LABELS[kind]} secondary={POST_KIND_HINTS[kind]} />
          </MenuItem>
        ))}
      </Menu>

      <Menu anchorEl={decorationMenu} open={Boolean(decorationMenu)} onClose={() => setDecorationMenu(null)}>
        {DECORATION_KINDS.map((kind) => (
          <MenuItem key={kind} onClick={() => addDecoration(kind)}>
            {DECORATION_LABELS[kind]}
          </MenuItem>
        ))}
      </Menu>

      <Dialog open={discarding} onClose={() => setDiscarding(false)}>
        <DialogTitle>Descartar alterações?</DialogTitle>
        <DialogContent>
          <DialogContentText>O mapa volta para a última versão salva.</DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDiscarding(false)}>Continuar editando</Button>
          <Button color="error" variant="contained" onClick={discard}>
            Descartar
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={savedAt} autoHideDuration={3000} onClose={() => setSavedAt(false)} message="Layout salvo" />
    </Stack>
  );
}
