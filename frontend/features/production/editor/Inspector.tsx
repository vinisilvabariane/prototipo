"use client";

import ContentCopy from "@mui/icons-material/ContentCopy";
import DeleteOutlined from "@mui/icons-material/DeleteOutlined";
import RotateRight from "@mui/icons-material/RotateRight";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import FormControlLabel from "@mui/material/FormControlLabel";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { inkSoft, line } from "@/lib/tokens";
import { MIN_DECORATION_SIZE, MIN_POST_HEIGHT, MIN_POST_WIDTH } from "../canvas/geometry";
import { isDecorationNode, isPostNode, type CanvasNode, type DecorationNodeData, type PostNodeData } from "../canvas/nodeData";
import { DECORATION_LABELS, FIELD_LIMITS, MAX_MACHINES_PER_PERSON, POST_KINDS, POST_KIND_HINTS, POST_KIND_LABELS } from "../labels";
import type { PostKind } from "../types";

type Props = {
  selected: CanvasNode[];
  problems: Map<string, string>;
  onPatchPost: (id: string, patch: Partial<PostNodeData>) => void;
  onPatchDecoration: (id: string, patch: Partial<DecorationNodeData>) => void;
  onResize: (id: string, width: number, height: number) => void;
  onDuplicate: () => void;
  onDelete: () => void;
};

const sizeOf = (node: CanvasNode) => ({ width: Math.round(node.width ?? node.measured?.width ?? 0), height: Math.round(node.height ?? node.measured?.height ?? 0) });

function Heading({ children }: { children: string }) {
  return (
    <Typography variant="overline" component="h3" sx={{ color: inkSoft }}>
      {children}
    </Typography>
  );
}

function Rotation({ value, onChange }: { value: number; onChange: (value: number) => void }) {
  return (
    <Stack direction="row" spacing={1}>
      <TextField
        label="Rotação (graus)"
        type="number"
        value={value}
        onChange={(event) => onChange(Math.min(359, Math.max(0, Math.round(Number(event.target.value) || 0))))}
        slotProps={{ htmlInput: { min: 0, max: 359, step: 15 } }}
        fullWidth
      />
      <Button variant="outlined" aria-label="Girar 90 graus" onClick={() => onChange((value + 90) % 360)} sx={{ minWidth: 48 }}>
        <RotateRight />
      </Button>
    </Stack>
  );
}

function Dimensions({ node, minWidth, minHeight, onResize }: { node: CanvasNode; minWidth: number; minHeight: number; onResize: Props["onResize"] }) {
  const { width, height } = sizeOf(node);

  return (
    <Stack direction="row" spacing={1}>
      <TextField label="Largura" type="number" value={width} onChange={(event) => onResize(node.id, Math.max(minWidth, Math.round(Number(event.target.value) || minWidth)), height)} slotProps={{ htmlInput: { min: minWidth, step: 30 } }} />
      <TextField label="Altura" type="number" value={height} onChange={(event) => onResize(node.id, width, Math.max(minHeight, Math.round(Number(event.target.value) || minHeight)))} slotProps={{ htmlInput: { min: minHeight, step: 30 } }} />
    </Stack>
  );
}

export default function Inspector({ selected, problems, onPatchPost, onPatchDecoration, onResize, onDuplicate, onDelete }: Props) {
  const actions = (
    <Stack direction="row" spacing={1}>
      <Button size="small" variant="outlined" startIcon={<ContentCopy fontSize="small" />} onClick={onDuplicate}>
        Duplicar
      </Button>
      <Button size="small" color="error" variant="outlined" startIcon={<DeleteOutlined fontSize="small" />} onClick={onDelete}>
        Excluir
      </Button>
    </Stack>
  );

  if (selected.length === 0) {
    return (
      <Stack spacing={1.5}>
        <Heading>Seleção</Heading>
        <Typography sx={{ color: inkSoft }}>Clique em um posto ou uma decoração para editar. Arraste para mover, use as alças para redimensionar e segure Ctrl para selecionar vários.</Typography>
      </Stack>
    );
  }

  if (selected.length > 1) {
    return (
      <Stack spacing={2}>
        <Heading>Seleção</Heading>
        <Typography>{selected.length} itens selecionados.</Typography>
        {actions}
      </Stack>
    );
  }

  const node = selected[0];

  if (isPostNode(node)) {
    const data = node.data;
    const problem = problems.get(node.id);

    return (
      <Stack spacing={2}>
        <Heading>Posto</Heading>
        <TextField
          label="Nome do posto"
          value={data.name}
          onChange={(event) => onPatchPost(node.id, { name: event.target.value })}
          error={Boolean(problem)}
          helperText={problem}
          slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.postName } }}
          fullWidth
        />
        <TextField select label="Tipo" value={data.kind} onChange={(event) => onPatchPost(node.id, { kind: event.target.value as PostKind })} helperText={POST_KIND_HINTS[data.kind].replace("5", String(MAX_MACHINES_PER_PERSON))} fullWidth>
          {POST_KINDS.map((kind) => (
            <MenuItem key={kind} value={kind}>
              {POST_KIND_LABELS[kind]}
            </MenuItem>
          ))}
        </TextField>
        <FormControlLabel
          control={<Switch checked={data.isCritical} onChange={(event) => onPatchPost(node.id, { isCritical: event.target.checked })} />}
          label="Posto crítico"
        />
        <Typography variant="body2" sx={{ color: inkSoft, mt: "-8px !important" }}>
          A produção só inicia com todo posto crítico ocupado.
        </Typography>
        <TextField
          label="Meta por hora"
          type="number"
          value={data.targetPerHour ?? ""}
          onChange={(event) => onPatchPost(node.id, { targetPerHour: event.target.value === "" ? null : Math.max(0, Number(event.target.value)) })}
          slotProps={{ htmlInput: { min: 0, step: 1 } }}
          helperText="Peças por hora. Base da produtividade do posto."
          fullWidth
        />
        <TextField
          label="Operação"
          value={data.operation ?? ""}
          onChange={(event) => onPatchPost(node.id, { operation: event.target.value || null })}
          slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.operation } }}
          multiline
          minRows={2}
          fullWidth
        />
        <Box sx={{ borderTop: `1px solid ${line}`, pt: 2 }}>
          <Stack spacing={2}>
            <Rotation value={data.rotation} onChange={(rotation) => onPatchPost(node.id, { rotation })} />
            <Dimensions node={node} minWidth={MIN_POST_WIDTH} minHeight={MIN_POST_HEIGHT} onResize={onResize} />
            {actions}
          </Stack>
        </Box>
      </Stack>
    );
  }

  if (isDecorationNode(node)) {
    const data = node.data;

    return (
      <Stack spacing={2}>
        <Heading>{DECORATION_LABELS[data.kind]}</Heading>
        <TextField label="Texto" value={data.label ?? ""} onChange={(event) => onPatchDecoration(node.id, { label: event.target.value || null })} slotProps={{ htmlInput: { maxLength: FIELD_LIMITS.label } }} fullWidth />
        <FormControlLabel control={<Switch checked={data.locked} onChange={(event) => onPatchDecoration(node.id, { locked: event.target.checked })} />} label="Travar no lugar" />
        <Rotation value={data.rotation} onChange={(rotation) => onPatchDecoration(node.id, { rotation })} />
        <Dimensions node={node} minWidth={MIN_DECORATION_SIZE} minHeight={MIN_DECORATION_SIZE} onResize={onResize} />
        {actions}
      </Stack>
    );
  }

  return null;
}
