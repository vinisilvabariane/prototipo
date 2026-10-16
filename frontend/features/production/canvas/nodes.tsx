"use client";

import { memo } from "react";
import { NodeResizer, useReactFlow, useStore, type NodeProps } from "@xyflow/react";
import Box from "@mui/material/Box";
import DecorationShape from "./DecorationShape";
import { MIN_DECORATION_SIZE, MIN_POST_HEIGHT, MIN_POST_WIDTH, snap } from "./geometry";
import type { DecorationNodeType, PostNodeType } from "./nodeData";
import PostCard from "./PostCard";

const sizeOf = (node: { width?: number; height?: number; measured?: { width?: number; height?: number } }, fallback: number) => ({
  width: node.width ?? node.measured?.width ?? fallback,
  height: node.height ?? node.measured?.height ?? fallback,
});

export const PostNode = memo(function PostNode({ id, data, selected }: NodeProps<PostNodeType>) {
  const { updateNode } = useReactFlow();
  const zoom = useStore((state) => Math.round(state.transform[2] * 20) / 20);

  return (
    <>
      {data.editable && (
        <NodeResizer
          isVisible={selected}
          minWidth={MIN_POST_WIDTH}
          minHeight={MIN_POST_HEIGHT}
          onResizeEnd={(_event, params) => updateNode(id, { width: snap(params.width), height: snap(params.height), position: { x: snap(params.x), y: snap(params.y) } })}
        />
      )}
      <PostCard data={data} selected={selected} zoom={zoom} />
    </>
  );
});

export const DecorationNode = memo(function DecorationNode({ id, data, selected, width, height }: NodeProps<DecorationNodeType>) {
  const { updateNode } = useReactFlow();
  const size = sizeOf({ width, height }, 120);

  return (
    <>
      {data.editable && !data.locked && (
        <NodeResizer
          isVisible={selected}
          minWidth={MIN_DECORATION_SIZE}
          minHeight={MIN_DECORATION_SIZE}
          onResizeEnd={(_event, params) => updateNode(id, { width: snap(params.width), height: snap(params.height), position: { x: snap(params.x), y: snap(params.y) } })}
        />
      )}
      <Box
        sx={{
          width: "100%",
          height: "100%",
          transform: data.rotation ? `rotate(${data.rotation}deg)` : undefined,
          transformOrigin: "center",
          outline: selected && data.editable ? "2px solid var(--mui-palette-primary-main)" : "none",
          outlineOffset: 2,
          borderRadius: "8px",
        }}
      >
        <DecorationShape kind={data.kind} label={data.label} width={size.width} height={size.height} />
      </Box>
    </>
  );
});
