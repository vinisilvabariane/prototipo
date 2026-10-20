"use client";

import { useState, useTransition } from "react";
import DeleteOutlined from "@mui/icons-material/DeleteOutlined";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft, line } from "@/lib/tokens";
import { deleteTemplate } from "../actions/sectorActions";
import { FIELD_LABELS } from "../schema";
import type { FieldTemplate } from "../types";

type Props = {
  open: boolean;
  templates: FieldTemplate[];
  onClose: () => void;
  onUse: (template: FieldTemplate) => void;
  onDeleted: (id: number) => void;
  canUse: boolean;
};

export default function TemplatesDialog({ open, templates, onClose, onUse, onDeleted, canUse }: Props) {
  const [error, setError] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  function remove(id: number) {
    setError(undefined);

    startTransition(async () => {
      const result = await deleteTemplate(id);
      if (result.ok) onDeleted(id);
      else setError(result.error);
    });
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Modelos de campo</DialogTitle>
      <DialogContent>
        <Stack spacing={1.5} sx={{ pt: 0.5 }}>
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Um modelo guarda tipo, título, obrigatoriedade e opções de uma pergunta para reaproveitar em qualquer setor. Usar um modelo copia os dados; mudar o modelo depois não altera formulários já montados.
          </Typography>
          {error && <ErrorAlert error={error} />}
          {templates.length === 0 && (
            <Typography variant="body2" sx={{ color: inkSoft, py: 2 }}>
              Nenhum modelo ainda. Monte uma pergunta e use “Salvar como modelo”.
            </Typography>
          )}
          {templates.map((template) => (
            <Stack key={template.id} direction="row" spacing={1} sx={{ alignItems: "center", border: `1px solid ${line}`, borderRadius: 1, px: 1.5, py: 1 }}>
              <Stack sx={{ flex: 1, minWidth: 0 }}>
                <Typography variant="body2" sx={{ fontWeight: 700 }} noWrap>
                  {template.name}
                </Typography>
                <Typography variant="caption" sx={{ color: inkSoft }} noWrap>
                  {FIELD_LABELS[template.type]} · {template.label}
                  {template.options.length > 0 && ` · ${template.options.length} opção(ões)`}
                </Typography>
              </Stack>
              {canUse && (
                <Button size="small" onClick={() => onUse(template)}>
                  Usar
                </Button>
              )}
              <IconButton size="small" color="error" aria-label={`Excluir modelo ${template.name}`} disabled={pending} onClick={() => remove(template.id)}>
                <DeleteOutlined fontSize="small" />
              </IconButton>
            </Stack>
          ))}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Fechar</Button>
      </DialogActions>
    </Dialog>
  );
}
