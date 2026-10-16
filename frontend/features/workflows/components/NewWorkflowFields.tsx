"use client";

import { useRef, useState, type ChangeEvent, type DragEvent } from "react";
import Close from "@mui/icons-material/Close";
import CloudUpload from "@mui/icons-material/CloudUpload";
import Alert from "@mui/material/Alert";
import Autocomplete from "@mui/material/Autocomplete";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { brand, inkSoft, lineStrong } from "@/lib/tokens";
import { formatBytes, todayInZone } from "../format";
import { MAX_ATTACHMENT_BYTES, MAX_ATTACHMENTS } from "../labels";
import type { WorkflowUser } from "../types";
import WorkflowFields from "./WorkflowFields";

const sameFile = (a: File, b: File) => a.name === b.name && a.size === b.size && a.lastModified === b.lastModified;

const userLabel = (user: WorkflowUser) => `${user.name} (${user.registration})`;

export default function NewWorkflowFields({ users }: { users: WorkflowUser[] }) {
  const [assignees, setAssignees] = useState<WorkflowUser[]>([]);
  const [files, setFiles] = useState<File[]>([]);
  const [notice, setNotice] = useState<string>();
  const [dragging, setDragging] = useState(false);
  const input = useRef<HTMLInputElement>(null);

  function commit(next: File[]) {
    setFiles(next);
    const transfer = new DataTransfer();
    for (const file of next) transfer.items.add(file);
    if (input.current) input.current.files = transfer.files;
  }

  function add(incoming: File[]) {
    const accepted = [...files];
    const problems: string[] = [];

    for (const file of incoming) {
      if (accepted.length >= MAX_ATTACHMENTS) {
        problems.push(`Você pode enviar no máximo ${MAX_ATTACHMENTS} arquivos.`);
        break;
      }
      if (file.size === 0) problems.push(`"${file.name}" está vazio.`);
      else if (file.size > MAX_ATTACHMENT_BYTES) problems.push(`"${file.name}" excede o tamanho máximo de 10 MB.`);
      else if (accepted.some((existing) => sameFile(existing, file))) problems.push(`"${file.name}" já foi selecionado.`);
      else accepted.push(file);
    }

    commit(accepted);
    setNotice(problems.length > 0 ? problems.join(" ") : undefined);
  }

  function onPick(e: ChangeEvent<HTMLInputElement>) {
    if (e.target.files && e.target.files.length > 0) add(Array.from(e.target.files));
  }

  function onDrop(e: DragEvent<HTMLElement>) {
    e.preventDefault();
    setDragging(false);
    add(Array.from(e.dataTransfer.files));
  }

  return (
    <>
      <WorkflowFields />

      <TextField
        label="Prazo final"
        name="dueDate"
        type="date"
        helperText="Data limite para conclusão do workflow."
        slotProps={{ inputLabel: { shrink: true }, htmlInput: { min: todayInZone() } }}
        fullWidth
      />

      <Autocomplete
        multiple
        options={users}
        value={assignees}
        onChange={(_, next) => setAssignees(next)}
        getOptionLabel={userLabel}
        isOptionEqualToValue={(option, value) => option.id === value.id}
        noOptionsText="Nenhum usuário encontrado"
        renderInput={(params) => (
          <TextField {...params} label="Responsáveis" helperText="Você já é responsável e aprova automaticamente ao criar." />
        )}
      />
      {assignees.map((user) => (
        <input key={user.id} type="hidden" name="assigneeIds" value={user.id} />
      ))}

      <Stack spacing={1}>
        <Box
          component="label"
          onDragOver={(e: DragEvent<HTMLElement>) => {
            e.preventDefault();
            setDragging(true);
          }}
          onDragLeave={() => setDragging(false)}
          onDrop={onDrop}
          sx={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            gap: 0.5,
            p: 2.5,
            textAlign: "center",
            cursor: "pointer",
            border: `1px dashed ${dragging ? brand : lineStrong}`,
            borderRadius: "12px",
            transition: "border-color 0.2s ease",
            "&:hover": { borderColor: brand },
          }}
        >
          <CloudUpload sx={{ color: inkSoft }} />
          <Typography>Arraste arquivos aqui ou clique para selecionar</Typography>
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Até {MAX_ATTACHMENTS} arquivos de {formatBytes(MAX_ATTACHMENT_BYTES)} cada
          </Typography>
          <input ref={input} type="file" name="attachments" multiple hidden onChange={onPick} />
        </Box>

        {notice && <Alert severity="warning">{notice}</Alert>}

        {files.map((file, index) => (
          <Stack key={`${file.name}-${file.lastModified}`} direction="row" spacing={1} sx={{ alignItems: "center", justifyContent: "space-between" }}>
            <Box sx={{ minWidth: 0 }}>
              <Typography noWrap sx={{ fontWeight: 600 }}>
                {file.name}
              </Typography>
              <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
                {formatBytes(file.size)}
              </Typography>
            </Box>
            <IconButton aria-label={`Remover ${file.name}`} size="small" onClick={() => commit(files.filter((_, position) => position !== index))}>
              <Close fontSize="small" />
            </IconButton>
          </Stack>
        ))}
        {files.length > 0 && (
          <Typography variant="body2" sx={{ color: inkSoft }}>
            {files.length} de {MAX_ATTACHMENTS} arquivos selecionados
          </Typography>
        )}
      </Stack>
    </>
  );
}
