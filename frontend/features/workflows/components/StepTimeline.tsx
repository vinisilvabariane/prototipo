import type { ElementType } from "react";
import Add from "@mui/icons-material/Add";
import Cancel from "@mui/icons-material/Cancel";
import Check from "@mui/icons-material/Check";
import CheckCircle from "@mui/icons-material/CheckCircle";
import Close from "@mui/icons-material/Close";
import EditOutlined from "@mui/icons-material/EditOutlined";
import PersonAdd from "@mui/icons-material/PersonAdd";
import SwapHoriz from "@mui/icons-material/SwapHoriz";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft, line, lineStrong } from "@/lib/tokens";
import { formatDateTime } from "../format";
import { ACTION_TITLES, REVIEW_NOTICES } from "../labels";
import type { WorkflowAction, WorkflowStep } from "../types";
import ChatButton from "./ChatButton";
import JustificationButton from "./JustificationButton";
import ReviewDialog from "./ReviewDialog";

const ICONS: Record<WorkflowAction, ElementType> = {
  created: Add,
  edited: EditOutlined,
  approval: Check,
  rejection: Close,
  finalApproval: CheckCircle,
  finalRejection: Cancel,
  assigneeAdded: PersonAdd,
  assigneeReplaced: SwapHoriz,
};

const NEGATIVE: WorkflowAction[] = ["rejection", "finalRejection"];

type Props = { workflowId: number; steps: WorkflowStep[] };

export default function StepTimeline({ workflowId, steps }: Props) {
  if (steps.length === 0) {
    return <Typography sx={{ color: inkSoft }}>Nenhuma ação registrada no histórico.</Typography>;
  }

  return (
    <Stack component="ol" sx={{ listStyle: "none", m: 0, p: 0 }}>
      {steps.map((step, index) => {
        const Icon = ICONS[step.action];
        const negative = NEGATIVE.includes(step.action);
        const pendingReview = step.review !== "none";
        const last = index === steps.length - 1;

        return (
          <Box key={step.id} component="li" sx={{ display: "flex", gap: 2, pb: last ? 0 : 3, position: "relative" }}>
            {!last && <Box aria-hidden sx={{ position: "absolute", left: 17, top: 36, bottom: 0, borderLeft: `1px solid ${line}` }} />}
            <Box
              aria-hidden
              sx={{
                flexShrink: 0,
                width: 36,
                height: 36,
                borderRadius: "50%",
                display: "grid",
                placeItems: "center",
                border: `1px solid ${negative ? "var(--mui-palette-error-main)" : lineStrong}`,
                color: negative ? "error.main" : "text.primary",
                bgcolor: "background.paper",
              }}
            >
              <Icon sx={{ fontSize: 18 }} />
            </Box>

            <Box sx={{ minWidth: 0, flex: 1 }}>
              <Stack direction="row" spacing={1} sx={{ justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap" }}>
                <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                  <Typography sx={{ fontWeight: 600 }}>{ACTION_TITLES[step.action]}</Typography>
                  {pendingReview && <Chip size="small" color="warning" label="Revisão pendente" />}
                </Stack>
                <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
                  {formatDateTime(step.createdAt)}
                </Typography>
              </Stack>

              <Typography variant="body2" sx={{ color: inkSoft, mt: 0.25 }}>
                {step.description}
              </Typography>
              <Typography variant="body2" sx={{ color: inkSoft }}>
                Por {step.userName}
              </Typography>

              {pendingReview && step.review !== "none" && (
                <Alert severity={step.review === "creatorReview" ? "warning" : "info"} sx={{ mt: 1.5 }}>
                  <strong>Revisão pendente:</strong> {REVIEW_NOTICES[step.review]}
                </Alert>
              )}

              <Stack direction="row" spacing={1} sx={{ mt: 1, ml: -1, flexWrap: "wrap" }}>
                {step.justification && (
                  <JustificationButton
                    title={ACTION_TITLES[step.action]}
                    author={step.userName}
                    date={formatDateTime(step.createdAt)}
                    text={step.justification}
                  />
                )}
                {step.action === "rejection" && (
                  <ChatButton
                    workflowId={workflowId}
                    stepId={step.id}
                    rejecter={step.userName}
                    rejectedAt={step.createdAt}
                    justification={step.justification}
                  />
                )}
                {step.actorRole !== "none" && <ReviewDialog workflowId={workflowId} stepId={step.id} role={step.actorRole} />}
              </Stack>
            </Box>
          </Box>
        );
      })}
    </Stack>
  );
}
