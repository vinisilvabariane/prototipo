"use client";

import { useEffect, useState, useTransition } from "react";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import List from "@mui/material/List";
import ListItemButton from "@mui/material/ListItemButton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import { allocatePost, searchCandidates } from "../actions/runActions";
import { MAX_MACHINES_PER_PERSON } from "../labels";
import type { PostKind, RunCandidate } from "../types";

type Props = {
  runId: number;
  postId: number;
  postName: string;
  postKind: PostKind;
  currentUserId: number | null;
  open: boolean;
  onClose: () => void;
};

function reasonOf(candidate: RunCandidate, kind: PostKind, currentUserId: number | null): string | null {
  if (candidate.busyLine) return `Em produção na linha ${candidate.busyLine}`;
  if (candidate.id === currentUserId) return "Já está neste posto";
  if (kind === "machine") {
    if (candidate.holdsCommonPost) return "Está em um posto comum";
    if (candidate.machinePosts >= MAX_MACHINES_PER_PERSON) return `Já cuida de ${MAX_MACHINES_PER_PERSON} máquinas`;
    return null;
  }
  if (candidate.holdsCommonPost || candidate.machinePosts > 0) return "Já está em outro posto desta produção";
  return null;
}

export default function PersonPicker({ runId, postId, postName, postKind, currentUserId, open, onClose }: Props) {
  const [search, setSearch] = useState("");
  const [candidates, setCandidates] = useState<RunCandidate[] | null>(null);
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  useEffect(() => {
    if (!open) return;

    let cancelled = false;

    const timer = setTimeout(async () => {
      const result = await searchCandidates(runId, search);
      if (cancelled) return;

      if (result.ok) {
        setCandidates(result.value);
        setError(undefined);
      } else {
        setError(result.error);
      }
    }, 250);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [open, runId, search]);

  function choose(candidate: RunCandidate) {
    setError(undefined);

    startTransition(async () => {
      const result = await allocatePost(runId, postId, candidate.id);

      if (!result.ok) {
        setError(result.error);
        return;
      }

      onClose();
    });
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Quem vai para {postName}?</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField autoFocus label="Buscar por nome ou matrícula" value={search} onChange={(event) => setSearch(event.target.value)} fullWidth />
          {error && <ErrorAlert error={error} />}
          <Box sx={{ minHeight: 240 }}>
            {candidates === null ? (
              <Box sx={{ display: "grid", placeItems: "center", py: 6 }}>
                <CircularProgress size={24} />
              </Box>
            ) : candidates.length === 0 ? (
              <Typography sx={{ color: inkSoft, py: 3, textAlign: "center" }}>Ninguém encontrado.</Typography>
            ) : (
              <List disablePadding sx={{ maxHeight: 360, overflowY: "auto" }}>
                {candidates.map((candidate) => {
                  const reason = reasonOf(candidate, postKind, currentUserId);

                  return (
                    <ListItemButton key={candidate.id} disabled={pending || reason !== null} onClick={() => choose(candidate)} sx={{ borderRadius: "10px", alignItems: "flex-start", gap: 1.5 }}>
                      <Box sx={{ minWidth: 0, flex: 1 }}>
                        <Typography noWrap sx={{ fontWeight: 600 }}>
                          {candidate.name}
                        </Typography>
                        <Typography variant="body2" noWrap sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
                          Mat. {candidate.registration}
                          {candidate.department && ` · ${candidate.department}`}
                        </Typography>
                        {reason && (
                          <Typography variant="body2" sx={{ color: "var(--mui-palette-warning-main)", mt: 0.25 }}>
                            {reason}
                          </Typography>
                        )}
                      </Box>
                      {candidate.machinePosts > 0 && <Chip size="small" variant="outlined" label={`${candidate.machinePosts}/${MAX_MACHINES_PER_PERSON} máq.`} />}
                    </ListItemButton>
                  );
                })}
              </List>
            )}
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancelar</Button>
      </DialogActions>
    </Dialog>
  );
}
