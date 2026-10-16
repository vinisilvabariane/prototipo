"use client";

import { useState, useTransition } from "react";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { brandSoft, inkSoft, line } from "@/lib/tokens";
import { loadStepMessages } from "../actions/workflowActions";
import { formatDateTime } from "../format";
import type { WorkflowMessage } from "../types";

type Props = {
  workflowId: number;
  stepId: number;
  rejecter: string;
  rejectedAt: string;
  justification: string | null;
};

type State = { messages?: WorkflowMessage[]; error?: AppError };

function Bubble({ author, date, text, highlighted }: { author: string; date: string; text: string; highlighted?: boolean }) {
  return (
    <Box sx={{ p: 1.5, borderRadius: "12px", border: `1px solid ${line}`, bgcolor: highlighted ? brandSoft : "transparent" }}>
      <Stack direction="row" spacing={1} sx={{ justifyContent: "space-between", mb: 0.5 }}>
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {author}
        </Typography>
        <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
          {date}
        </Typography>
      </Stack>
      <Typography sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>{text}</Typography>
    </Box>
  );
}

export default function ChatButton({ workflowId, stepId, rejecter, rejectedAt, justification }: Props) {
  const [open, setOpen] = useState(false);
  const [state, setState] = useState<State>({});
  const [pending, startTransition] = useTransition();

  function show() {
    setOpen(true);
    setState({});
    startTransition(async () => {
      const result = await loadStepMessages(workflowId, stepId);
      setState(result.ok ? { messages: result.value } : { error: result.error });
    });
  }

  return (
    <>
      <Button size="small" onClick={show}>
        Conversa
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Conversa sobre a rejeição</DialogTitle>
        <DialogContent>
          <Stack spacing={1.5} sx={{ pt: 1 }}>
            {justification && <Bubble highlighted author={`${rejecter} (rejeição)`} date={formatDateTime(rejectedAt)} text={justification} />}
            {pending && (
              <Box sx={{ display: "flex", justifyContent: "center", py: 2 }}>
                <CircularProgress size={24} />
              </Box>
            )}
            {state.error && <ErrorAlert error={state.error} />}
            {state.messages?.map((message) => (
              <Bubble key={message.id} author={message.userName} date={formatDateTime(message.createdAt)} text={message.text} />
            ))}
            {state.messages?.length === 0 && (
              <Typography variant="body2" sx={{ color: inkSoft }}>
                Ninguém respondeu a esta rejeição ainda. As respostas do criador e de quem rejeitou aparecem aqui.
              </Typography>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Fechar</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
