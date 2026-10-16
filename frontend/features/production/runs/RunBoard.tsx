"use client";

import { useMemo, useState, useTransition } from "react";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft, line } from "@/lib/tokens";
import { releasePost } from "../actions/runActions";
import CanvasView from "../canvas/CanvasView";
import { decorationNodeId, postNodeId, type CanvasNode } from "../canvas/nodeData";
import { formatDateTime, formatMinutes, formatPercent, formatQuantity } from "../format";
import { POST_KIND_LABELS } from "../labels";
import type { RunDetail, RunPost } from "../types";
import PersonPicker from "./PersonPicker";
import RecordOutputDialog from "./RecordOutputDialog";

function Row({ label, value }: { label: string; value: string }) {
  return (
    <Box sx={{ display: "flex", justifyContent: "space-between", gap: 2, py: 0.75, borderBottom: `1px solid ${line}`, "&:last-of-type": { borderBottom: 0 } }}>
      <Typography variant="body2" sx={{ color: inkSoft }}>
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 600, fontVariantNumeric: "tabular-nums", textAlign: "right" }}>
        {value}
      </Typography>
    </Box>
  );
}

function toNodes(run: RunDetail, selectedId: number | null): CanvasNode[] {
  const showMetrics = run.status !== "preparing";

  return [
    ...run.decorations.map<CanvasNode>((decoration, index) => ({
      id: decorationNodeId(index + 1),
      type: "decoration",
      position: { x: decoration.x, y: decoration.y },
      width: decoration.width,
      height: decoration.height,
      zIndex: 0,
      draggable: false,
      selectable: false,
      data: { kind: decoration.kind, label: decoration.label, rotation: decoration.rotation, locked: true, editable: false },
    })),
    ...run.posts.map<CanvasNode>((post) => ({
      id: postNodeId(post.id),
      type: "post",
      position: { x: post.x, y: post.y },
      width: post.width,
      height: post.height,
      zIndex: 1,
      draggable: false,
      selected: post.id === selectedId,
      data: {
        kind: post.kind,
        name: post.name,
        isCritical: post.isCritical,
        targetPerHour: post.targetPerHour,
        operation: post.operation,
        rotation: post.rotation,
        editable: false,
        holder: post.allocation ? { name: post.allocation.userName, registration: post.allocation.registration } : null,
        showMetrics,
        produced: post.produced,
        expected: post.expected,
        attainment: post.attainment,
        invalid: false,
      },
    })),
  ];
}

function PostPanel({ run, post, onPick, onRecord }: { run: RunDetail; post: RunPost; onPick: () => void; onRecord: () => void }) {
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();
  const canChange = run.canSupervise && run.status !== "closed";

  function release() {
    setError(undefined);

    startTransition(async () => {
      const result = await releasePost(run.id, post.id);
      if (!result.ok) setError(result.error);
    });
  }

  return (
    <Stack spacing={2}>
      <Box>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 0.5 }}>
          <Typography variant="h6" component="h3" sx={{ overflowWrap: "anywhere" }}>
            {post.name}
          </Typography>
          {post.isCritical && <Chip size="small" color="error" label="Crítico" />}
        </Stack>
        <Typography variant="body2" sx={{ color: inkSoft }}>
          {POST_KIND_LABELS[post.kind]}
          {post.operation && ` · ${post.operation}`}
        </Typography>
      </Box>

      <Box>
        <Typography variant="overline" component="h4" sx={{ color: inkSoft }}>
          Quem está aqui
        </Typography>
        {post.allocation ? (
          <Box sx={{ mt: 0.5 }}>
            <Typography sx={{ fontWeight: 600 }}>{post.allocation.userName}</Typography>
            <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
              Mat. {post.allocation.registration} · desde {formatDateTime(post.allocation.allocatedAt)}
            </Typography>
          </Box>
        ) : (
          <Typography sx={{ color: inkSoft, mt: 0.5 }}>Ninguém alocado.</Typography>
        )}
        {canChange && (
          <Stack direction="row" spacing={1} sx={{ mt: 1.5 }}>
            <Button variant="contained" size="small" onClick={onPick} disabled={pending}>
              {post.allocation ? "Trocar pessoa" : "Alocar pessoa"}
            </Button>
            {post.allocation && (
              <Button size="small" color="inherit" onClick={release} disabled={pending}>
                Liberar posto
              </Button>
            )}
          </Stack>
        )}
      </Box>

      {run.status !== "preparing" && (
        <Box>
          <Typography variant="overline" component="h4" sx={{ color: inkSoft }}>
            Produtividade
          </Typography>
          <Box sx={{ mt: 0.5 }}>
            <Row label="Meta por hora" value={formatQuantity(post.targetPerHour)} />
            <Row label="Tempo ocupado" value={formatMinutes(post.occupiedMinutes)} />
            <Row label="Esperado no período" value={formatQuantity(post.expected)} />
            <Row label="Produzido" value={formatQuantity(post.produced)} />
            <Row label="Atingimento" value={formatPercent(post.attainment)} />
          </Box>
          {run.canRecord && run.status === "running" && (
            <Button variant="outlined" size="small" onClick={onRecord} sx={{ mt: 1.5 }}>
              Lançar produção
            </Button>
          )}
        </Box>
      )}

      {error && <ErrorAlert error={error} />}
    </Stack>
  );
}

export default function RunBoard({ run }: { run: RunDetail }) {
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [picking, setPicking] = useState(false);
  const [recording, setRecording] = useState(false);

  const nodes = useMemo(() => toNodes(run, selectedId), [run, selectedId]);
  const selected = run.posts.find((post) => post.id === selectedId) ?? null;
  const occupied = run.posts.filter((post) => post.allocation).length;
  const missing = run.posts.filter((post) => post.isCritical && !post.allocation);

  return (
    <Box sx={{ display: "grid", gap: 2, gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "minmax(0, 1fr) 320px" }, alignItems: "start" }}>
      <CanvasView
        nodes={nodes}
        editable={false}
        onNodeClick={(id) => {
          if (id.startsWith("post:")) setSelectedId(Number(id.slice(5)));
        }}
        onPaneClick={() => setSelectedId(null)}
      />

      <Card sx={{ p: 2.5, position: { lg: "sticky" }, top: { lg: 80 } }}>
        {selected ? (
          <PostPanel run={run} post={selected} onPick={() => setPicking(true)} onRecord={() => setRecording(true)} />
        ) : (
          <Stack spacing={1.5}>
            <Typography variant="overline" component="h3" sx={{ color: inkSoft }}>
              Resumo
            </Typography>
            <Box>
              <Row label="Postos ocupados" value={`${occupied} de ${run.posts.length}`} />
              <Row label="Pessoas no mapa" value={String(new Set(run.posts.filter((post) => post.allocation).map((post) => post.allocation!.userId)).size)} />
              {run.status !== "preparing" && <Row label="Produzido" value={formatQuantity(run.totalProduced)} />}
            </Box>
            {missing.length > 0 && run.status !== "closed" && (
              <Alert severity="warning">Postos críticos sem ninguém: {missing.map((post) => post.name).join(", ")}.</Alert>
            )}
            <Typography variant="body2" sx={{ color: inkSoft }}>
              Clique em um posto do mapa para ver quem está nele{run.canSupervise && run.status !== "closed" ? ", alocar ou trocar a pessoa" : ""}
              {run.status === "running" && run.canRecord ? " e lançar a produção da hora" : ""}.
            </Typography>
          </Stack>
        )}
      </Card>

      {selected && (
        <>
          <PersonPicker
            runId={run.id}
            postId={selected.id}
            postName={selected.name}
            postKind={selected.kind}
            currentUserId={selected.allocation?.userId ?? null}
            open={picking}
            onClose={() => setPicking(false)}
          />
          {run.startedAt && <RecordOutputDialog runId={run.id} postId={selected.id} postName={selected.name} startedAt={run.startedAt} open={recording} onClose={() => setRecording(false)} />}
        </>
      )}
    </Box>
  );
}
