import Chip from "@mui/material/Chip";
import DataTable from "@/components/ui/DataTable";
import { POST_KIND_LABELS } from "@/features/production/labels";
import { formatDuration, formatQuantity } from "../format";
import type { OeePost } from "../types";
import OeeBar from "./OeeBar";

type Props = { posts: OeePost[]; lastPostId: number | null };

export default function PostOeeTable({ posts, lastPostId }: Props) {
  const rows = [...posts].sort((a, b) => a.x - b.x || a.postId - b.postId);

  return (
    <DataTable
      rows={rows}
      rowKey={(post) => post.postId}
      emptyTitle="Esta linha não tem postos"
      columns={[
        {
          key: "name",
          header: "Posto",
          strong: true,
          render: (post) => (
            <>
              {post.name}
              {post.postId === lastPostId && <Chip size="small" variant="outlined" label="Último do fluxo" sx={{ ml: 1 }} />}
            </>
          ),
        },
        { key: "kind", header: "Tipo", fit: true, hideBelow: "md", render: (post) => POST_KIND_LABELS[post.kind] },
        {
          key: "target",
          header: "Meta/h",
          fit: true,
          numeric: true,
          hideBelow: "md",
          render: (post) => (post.targetPerHour === null ? "Sem meta" : formatQuantity(post.targetPerHour)),
        },
        {
          key: "produced",
          header: "Produzido",
          fit: true,
          numeric: true,
          render: (post) => (post.measured ? `${formatQuantity(post.produced)} de ${formatQuantity(post.theoretical)}` : "—"),
        },
        { key: "occupied", header: "Com gente", fit: true, numeric: true, hideBelow: "lg", render: (post) => formatDuration(post.occupiedSeconds) },
        {
          key: "availability",
          header: "Disp.",
          fit: true,
          numeric: true,
          hideBelow: "lg",
          render: (post) => (post.measured ? <OeeBar value={post.pillars.availability} width={40} tone="neutral" /> : "—"),
        },
        {
          key: "performance",
          header: "Desemp.",
          fit: true,
          numeric: true,
          hideBelow: "lg",
          render: (post) => (post.measured ? <OeeBar value={post.pillars.performance} width={40} tone="neutral" /> : "—"),
        },
        {
          key: "quality",
          header: "Qual.",
          fit: true,
          numeric: true,
          hideBelow: "lg",
          render: (post) => (post.measured ? <OeeBar value={post.pillars.quality} width={40} tone="neutral" /> : "—"),
        },
        { key: "oee", header: "OEE", fit: true, numeric: true, strong: true, render: (post) => (post.measured ? <OeeBar value={post.pillars.oee} strong /> : "Sem meta") },
      ]}
    />
  );
}
