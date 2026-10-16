"use client";

import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import FormDialog from "@/components/ui/FormDialog";
import type { Department } from "@/features/departments/types";
import { inkSoft, line } from "@/lib/tokens";
import {
  assignHolder,
  createPosition,
  deletePosition,
  movePosition,
  updatePosition,
  vacatePosition,
} from "../actions/orgChartActions";
import { allowedParents, canAddBelow, chainOf, countBelow, directReports, indexById, buildTree } from "../tree";
import type { OrgPosition, OrgUser } from "../types";
import PositionFields from "./PositionFields";

type Props = {
  position: OrgPosition;
  positions: OrgPosition[];
  departments: Department[];
  users: OrgUser[];
  canManage: boolean;
  onSelect: (id: number) => void;
};

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <Box sx={{ display: "flex", justifyContent: "space-between", gap: 2, py: 1.25, borderBottom: `1px solid ${line}`, "&:last-of-type": { borderBottom: 0 } }}>
      <Typography sx={{ color: inkSoft }}>{label}</Typography>
      <Typography component="div" sx={{ fontWeight: 600, textAlign: "right", overflowWrap: "anywhere" }}>
        {children}
      </Typography>
    </Box>
  );
}

export default function PositionPanel({ position, positions, departments, users, canManage, onSelect }: Props) {
  const byId = indexById(positions);
  const chain = chainOf(byId, position.id);
  const reports = directReports(positions, position.id);
  const node = findNode(positions, position.id);
  const below = node ? countBelow(node) : 0;
  const parents = allowedParents(positions, position.id);
  const holder = position.holder;

  return (
    <Card sx={{ p: 3 }}>
      <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
        Cargo selecionado
      </Typography>
      <Typography variant="h5" component="h3" sx={{ mt: 0.5, overflowWrap: "anywhere" }}>
        {position.title}
      </Typography>

      <Box sx={{ mt: 1.5 }}>
        <Row label="Departamento">{position.department}</Row>
        <Row label="Ocupante">
          {holder ? (
            <>
              {holder.name}
              {!holder.active && <Chip size="small" label="Inativo" color="warning" sx={{ ml: 1 }} />}
            </>
          ) : (
            <Box component="span" sx={{ color: inkSoft, fontStyle: "italic", fontWeight: 400 }}>
              Vago
            </Box>
          )}
        </Row>
        {holder && <Row label="Matrícula"><Box component="span" sx={{ fontVariantNumeric: "tabular-nums" }}>{holder.registration}</Box></Row>}
        {holder?.email && <Row label="E-mail">{holder.email}</Row>}
        <Row label="Subordinados diretos">{reports.length}</Row>
        <Row label="Equipe total">{below}</Row>
      </Box>

      {chain.length > 1 && (
        <Box sx={{ mt: 2 }}>
          <Typography variant="overline" component="h4" sx={{ color: inkSoft }}>
            Cadeia de comando
          </Typography>
          <Stack direction="row" sx={{ flexWrap: "wrap", alignItems: "center", rowGap: 0.5, mt: 0.5 }}>
            {chain.map((item, index) => (
              <Box key={item.id} sx={{ display: "flex", alignItems: "center" }}>
                {index > 0 && (
                  <Typography component="span" aria-hidden sx={{ color: inkSoft, mx: 0.5 }}>
                    ›
                  </Typography>
                )}
                {item.id === position.id ? (
                  <Typography component="span" sx={{ fontWeight: 600, fontSize: "0.85rem" }}>
                    {item.title}
                  </Typography>
                ) : (
                  <Button size="small" onClick={() => onSelect(item.id)} sx={{ minWidth: 0, px: 0.75 }}>
                    {item.title}
                  </Button>
                )}
              </Box>
            ))}
          </Stack>
        </Box>
      )}

      {reports.length > 0 && (
        <Box sx={{ mt: 2 }}>
          <Typography variant="overline" component="h4" sx={{ color: inkSoft }}>
            Subordinados diretos
          </Typography>
          <Box component="ul" sx={{ listStyle: "none", m: 0, p: 0 }}>
            {reports.map((report) => (
              <Box key={report.id} component="li">
                <Button size="small" onClick={() => onSelect(report.id)} sx={{ justifyContent: "flex-start", textAlign: "left", width: "100%" }}>
                  {report.title} · {report.holder ? report.holder.name : "Vago"}
                </Button>
              </Box>
            ))}
          </Box>
        </Box>
      )}

      {canManage && (
        <Stack direction="row" spacing={1} sx={{ mt: 3, flexWrap: "wrap", rowGap: 1 }}>
          {canAddBelow(positions, position.id) && (
            <FormDialog
              title={`Novo cargo abaixo de ${position.title}`}
              trigger="Adicionar subordinado"
              submitLabel="Criar cargo"
              action={createPosition.bind(null, position.id)}
            >
              <PositionFields departments={departments} defaultDepartmentId={position.departmentId} holders={users} />
            </FormDialog>
          )}
          <FormDialog compact title="Editar cargo" trigger="Editar" action={updatePosition.bind(null, position.id)}>
            <PositionFields departments={departments} position={position} />
          </FormDialog>
          {holder ? (
            <ConfirmVacate id={position.id} title={position.title} name={holder.name} />
          ) : null}
          <FormDialog
            compact
            title={holder ? "Trocar ocupante" : "Definir ocupante"}
            trigger={holder ? "Trocar ocupante" : "Definir ocupante"}
            action={assignHolder.bind(null, position.id)}
          >
            <TextField select label="Colaborador" name="userId" defaultValue="" helperText="Só aparecem colaboradores ativos que ainda não ocupam um cargo." fullWidth>
              {users.map((user) => (
                <MenuItem key={user.id} value={user.id}>
                  {user.name} ({user.registration}) · {user.department}
                </MenuItem>
              ))}
            </TextField>
          </FormDialog>
          <FormDialog compact title="Mover cargo" trigger="Mover" submitLabel="Mover" action={movePosition.bind(null, position.id)}>
            <TextField select label="Novo cargo superior" name="parentId" defaultValue={position.parentId ?? ""} helperText="A equipe do cargo vai junto." fullWidth>
              <MenuItem value="">Nenhum (cargo de topo)</MenuItem>
              {parents.map((candidate) => (
                <MenuItem key={candidate.id} value={candidate.id}>
                  {candidate.title}
                  {candidate.holder ? ` · ${candidate.holder.name}` : ""}
                </MenuItem>
              ))}
            </TextField>
          </FormDialog>
          {reports.length === 0 && <ConfirmDeleteButton name={position.title} action={deletePosition.bind(null, position.id)} />}
        </Stack>
      )}
    </Card>
  );
}

function findNode(positions: OrgPosition[], id: number) {
  const stack = buildTree(positions);

  while (stack.length > 0) {
    const current = stack.pop()!;
    if (current.position.id === id) return current;
    stack.push(...current.children);
  }

  return undefined;
}

function ConfirmVacate({ id, title, name }: { id: number; title: string; name: string }) {
  return (
    <FormDialog compact title={`Desocupar ${title}`} trigger="Desocupar" submitLabel="Desocupar" color="error" action={vacatePosition.bind(null, id)}>
      <Typography>
        {name} deixa de ocupar este cargo. O cargo continua no organograma como vago.
      </Typography>
    </FormDialog>
  );
}
