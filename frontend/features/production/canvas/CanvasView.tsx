"use client";

import "@xyflow/react/dist/style.css";
import { useCallback } from "react";
import {
  Background,
  BackgroundVariant,
  Panel,
  ReactFlow,
  ReactFlowProvider,
  useReactFlow,
  useStore,
  type NodeChange,
  type NodeTypes,
  type OnSelectionChangeParams,
  type ReactFlowInstance,
} from "@xyflow/react";
import Add from "@mui/icons-material/Add";
import FitScreen from "@mui/icons-material/FitScreen";
import Remove from "@mui/icons-material/Remove";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import { canvas, inkSoft, line, surface } from "@/lib/tokens";
import { GRID } from "./geometry";
import type { CanvasNode } from "./nodeData";
import { DecorationNode, PostNode } from "./nodes";

const nodeTypes: NodeTypes = { post: PostNode, decoration: DecorationNode };

function ZoomControls() {
  const { zoomIn, zoomOut, fitView } = useReactFlow();
  const zoom = useStore((state) => state.transform[2]);

  return (
    <Panel position="bottom-left" style={{ margin: 12 }}>
      <Stack direction="row" spacing={0.25} sx={{ alignItems: "center", p: 0.5, borderRadius: "10px", bgcolor: surface, border: `1px solid ${line}` }}>
        <Tooltip title="Reduzir">
          <IconButton size="small" aria-label="Reduzir zoom" onClick={() => zoomOut({ duration: 150 })}>
            <Remove fontSize="small" />
          </IconButton>
        </Tooltip>
        <Typography sx={{ minWidth: 44, textAlign: "center", fontSize: 12, fontWeight: 600, color: inkSoft, fontVariantNumeric: "tabular-nums" }}>{Math.round(zoom * 100)}%</Typography>
        <Tooltip title="Ampliar">
          <IconButton size="small" aria-label="Ampliar zoom" onClick={() => zoomIn({ duration: 150 })}>
            <Add fontSize="small" />
          </IconButton>
        </Tooltip>
        <Tooltip title="Ver tudo">
          <IconButton size="small" aria-label="Ver o mapa inteiro" onClick={() => fitView({ duration: 250, padding: 0.12 })}>
            <FitScreen fontSize="small" />
          </IconButton>
        </Tooltip>
      </Stack>
    </Panel>
  );
}

type Props = {
  nodes: CanvasNode[];
  editable: boolean;
  height?: string | number;
  onNodesChange?: (changes: NodeChange<CanvasNode>[]) => void;
  onSelectionChange?: (ids: string[]) => void;
  onNodeClick?: (id: string) => void;
  onPaneClick?: () => void;
  onInit?: (instance: ReactFlowInstance<CanvasNode>) => void;
};

function Canvas({ nodes, editable, height, onNodesChange, onSelectionChange, onNodeClick, onPaneClick, onInit }: Props) {
  const handleSelection = useCallback(
    ({ nodes: selected }: OnSelectionChangeParams) => onSelectionChange?.(selected.map((node) => node.id)),
    [onSelectionChange],
  );

  return (
    <Box sx={{ position: "relative", height: height ?? "calc(100dvh - 260px)", minHeight: 460, borderRadius: "14px", overflow: "hidden", border: `1px solid ${line}`, bgcolor: canvas, "& .react-flow__node": { cursor: editable ? "grab" : "pointer" } }}>
      <ReactFlow<CanvasNode>
        nodes={nodes}
        edges={[]}
        nodeTypes={nodeTypes}
        onNodesChange={onNodesChange}
        onSelectionChange={handleSelection}
        onNodeClick={(_event, node) => onNodeClick?.(node.id)}
        onPaneClick={onPaneClick}
        onInit={onInit}
        snapToGrid
        snapGrid={[GRID, GRID]}
        minZoom={0.15}
        maxZoom={1.6}
        nodesDraggable={editable}
        nodesConnectable={false}
        elementsSelectable
        selectionOnDrag={editable}
        panOnDrag={editable ? [1, 2] : true}
        multiSelectionKeyCode={["Control", "Meta", "Shift"]}
        deleteKeyCode={null}
        fitView
        fitViewOptions={{ padding: 0.12, maxZoom: 1 }}
        proOptions={{ hideAttribution: true }}
      >
        <Background variant={BackgroundVariant.Dots} gap={GRID} size={1.4} color="var(--mui-palette-divider)" />
        <ZoomControls />
      </ReactFlow>
    </Box>
  );
}

export default function CanvasView(props: Props) {
  return (
    <ReactFlowProvider>
      <Canvas {...props} />
    </ReactFlowProvider>
  );
}
