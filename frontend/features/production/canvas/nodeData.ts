import type { Node } from "@xyflow/react";
import type { DecorationKind, PostKind } from "../types";

export type PostNodeData = {
  kind: PostKind;
  name: string;
  isCritical: boolean;
  targetPerHour: number | null;
  operation: string | null;
  rotation: number;
  editable: boolean;
  holder: { name: string; registration: number } | null;
  showMetrics: boolean;
  produced: number;
  expected: number | null;
  attainment: number | null;
  invalid: boolean;
  [key: string]: unknown;
};

export type DecorationNodeData = {
  kind: DecorationKind;
  label: string | null;
  rotation: number;
  locked: boolean;
  editable: boolean;
  [key: string]: unknown;
};

export type PostNodeType = Node<PostNodeData, "post">;
export type DecorationNodeType = Node<DecorationNodeData, "decoration">;
export type CanvasNode = PostNodeType | DecorationNodeType;

export const postNodeId = (id: number) => `post:${id}`;
export const decorationNodeId = (id: number) => `decoration:${id}`;

export const isPostNode = (node: CanvasNode): node is PostNodeType => node.type === "post";
export const isDecorationNode = (node: CanvasNode): node is DecorationNodeType => node.type === "decoration";

export const modelId = (node: CanvasNode) => Number(node.id.slice(node.id.indexOf(":") + 1));
