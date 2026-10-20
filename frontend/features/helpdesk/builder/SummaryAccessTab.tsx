"use client";

import Box from "@mui/material/Box";
import Checkbox from "@mui/material/Checkbox";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft, line } from "@/lib/tokens";
import { FIELD_LABELS, isSpecial } from "../schema";
import type { FormField, HelpdeskRole } from "../types";

type Props = {
  fields: FormField[];
  summaryFields: string[];
  onSummary: (ids: string[]) => void;
  roles: HelpdeskRole[];
  attendantRoleIds: number[];
  onRoles: (ids: number[]) => void;
};

const MAX_SUMMARY = 3;

export default function SummaryAccessTab({ fields, summaryFields, onSummary, roles, attendantRoleIds, onRoles }: Props) {
  const questions = fields.filter((field) => !isSpecial(field.type));

  function toggleSummary(id: string) {
    onSummary(summaryFields.includes(id) ? summaryFields.filter((item) => item !== id) : [...summaryFields, id].slice(0, MAX_SUMMARY));
  }

  function toggleRole(id: number) {
    onRoles(attendantRoleIds.includes(id) ? attendantRoleIds.filter((item) => item !== id) : [...attendantRoleIds, id]);
  }

  return (
    <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" }, alignItems: "start" }}>
      <Box sx={{ border: `1px solid ${line}`, borderRadius: 1.5, p: 2 }}>
        <Typography variant="overline" component="h3" sx={{ color: inkSoft }}>
          Resumo no quadro · {summaryFields.length} de {MAX_SUMMARY}
        </Typography>
        <Typography variant="body2" sx={{ color: inkSoft, mb: 1.5 }}>
          Escolha até três perguntas para aparecer no card do chamado, para o técnico decidir sem abrir.
        </Typography>
        <Stack>
          {questions.length === 0 && (
            <Typography variant="body2" sx={{ color: inkSoft }}>
              Monte as perguntas do formulário primeiro.
            </Typography>
          )}
          {questions.map((field) => {
            const checked = summaryFields.includes(field.id);

            return (
              <FormControlLabel
                key={field.id}
                control={<Checkbox checked={checked} disabled={!checked && summaryFields.length >= MAX_SUMMARY} onChange={() => toggleSummary(field.id)} />}
                label={
                  <>
                    {field.label}
                    <Typography component="span" variant="caption" sx={{ color: inkSoft, ml: 1 }}>
                      {FIELD_LABELS[field.type]}
                    </Typography>
                  </>
                }
              />
            );
          })}
        </Stack>
      </Box>

      <Box sx={{ border: `1px solid ${line}`, borderRadius: 1.5, p: 2 }}>
        <Typography variant="overline" component="h3" sx={{ color: inkSoft }}>
          Quem atende · {attendantRoleIds.length}
        </Typography>
        <Typography variant="body2" sx={{ color: inkSoft, mb: 1.5 }}>
          Pessoas com algum destes perfis veem o setor no quadro e podem assumir, recusar e finalizar chamados. Administradores atendem todos os setores.
        </Typography>
        <Stack>
          {roles.map((role) => (
            <FormControlLabel
              key={role.id}
              control={<Checkbox checked={attendantRoleIds.includes(role.id)} onChange={() => toggleRole(role.id)} />}
              label={
                <>
                  {role.name}
                  {role.description && (
                    <Typography component="span" variant="caption" sx={{ color: inkSoft, ml: 1 }}>
                      {role.description}
                    </Typography>
                  )}
                </>
              }
            />
          ))}
        </Stack>
      </Box>
    </Box>
  );
}
