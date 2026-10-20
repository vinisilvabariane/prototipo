import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import FormDialog from "@/components/ui/FormDialog";
import Panel from "@/components/ui/Panel";
import { inkSoft, line } from "@/lib/tokens";
import { addAssignee, replaceAssignee } from "../actions/workflowActions";
import type { WorkflowAssignee, WorkflowUser } from "../types";

type Props = {
  workflowId: number;
  creatorId: number;
  assignees: WorkflowAssignee[];
  candidates: WorkflowUser[];
  canManage: boolean;
};

function UserSelect({ name, label, users }: { name: string; label: string; users: WorkflowUser[] }) {
  return (
    <TextField select label={label} name={name} defaultValue="" fullWidth>
      {users.map((user) => (
        <MenuItem key={user.id} value={user.id}>
          {user.name} ({user.registration})
        </MenuItem>
      ))}
    </TextField>
  );
}

export default function AssigneesCard({ workflowId, creatorId, assignees, candidates, canManage }: Props) {
  const listed = new Set(assignees.map((assignee) => assignee.userId));
  const available = candidates.filter((user) => !listed.has(user.id));

  return (
    <Panel
      title="Responsáveis"
      count={assignees.length}
      action={
        canManage && (
          <FormDialog compact variant="outlined" title="Adicionar responsável" trigger="Adicionar" submitLabel="Adicionar" action={addAssignee.bind(null, workflowId)}>
            <UserSelect name="userId" label="Usuário" users={available} />
          </FormDialog>
        )
      }
    >
      <Box component="ul" sx={{ listStyle: "none", m: 0, p: 0 }}>
        {assignees.map((assignee) => {
          const isCreator = assignee.userId === creatorId;

          return (
            <Box
              key={assignee.userId}
              component="li"
              sx={{ py: 1, borderBottom: `1px solid ${line}`, "&:last-of-type": { borderBottom: 0 }, display: "flex", gap: 1, alignItems: "center", justifyContent: "space-between" }}
            >
              <Box sx={{ minWidth: 0 }}>
                <Typography noWrap sx={{ fontWeight: 600 }}>
                  {assignee.name}
                </Typography>
                <Typography variant="body2" sx={{ color: inkSoft }}>
                  Matrícula <Box component="span" sx={{ fontVariantNumeric: "tabular-nums" }}>{assignee.registration}</Box>
                  {isCreator ? " · Criador" : ""}
                </Typography>
              </Box>
              <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexShrink: 0 }}>
                <Chip size="small" variant={assignee.approved ? "filled" : "outlined"} color={assignee.approved ? "primary" : "default"} label={assignee.approved ? "Aprovou" : "Pendente"} />
                {canManage && !isCreator && !assignee.approved && (
                  <FormDialog
                    compact
                    title={`Substituir ${assignee.name}`}
                    trigger="Substituir"
                    submitLabel="Substituir"
                    action={replaceAssignee.bind(null, workflowId, assignee.userId)}
                  >
                    <Typography variant="body2" sx={{ color: inkSoft }}>
                      Quem for substituído deixa de ser responsável. É obrigatório escolher o novo responsável.
                    </Typography>
                    <UserSelect name="newUserId" label="Novo responsável" users={available} />
                  </FormDialog>
                )}
              </Stack>
            </Box>
          );
        })}
      </Box>
    </Panel>
  );
}
