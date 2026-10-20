"use client";

import { useCallback, useMemo, useState, useTransition } from "react";
import AddOutlined from "@mui/icons-material/AddOutlined";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import ButtonBase from "@mui/material/ButtonBase";
import Card from "@mui/material/Card";
import Chip from "@mui/material/Chip";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import ErrorAlert from "@/components/ui/ErrorAlert";
import { brand, brandSoft, brandWash, inkSoft, line } from "@/lib/tokens";
import type { AppError } from "@/lib/result";
import { formatDateTime } from "../format";
import { activateFormulaSet, createFormulaSet, deleteFormulaSet, updateFormulaSet } from "../actions/formulaActions";
import type { OeeConfig, OeeFormulas, SlotKey } from "../types";
import SlotEditor, { type TestRun, type TestSource } from "./SlotEditor";

type Selection = "default" | "new" | number;

type Props = { config: OeeConfig; testRuns: TestRun[] };

export default function FormulaStudio({ config, testRuns }: Props) {
  const [selection, setSelection] = useState<Selection>(config.activeSetId ?? "default");
  const [name, setName] = useState(() => config.sets.find((set) => set.id === config.activeSetId)?.name ?? "Padrão do sistema");
  const [formulas, setFormulas] = useState<OeeFormulas>(() => config.sets.find((set) => set.id === config.activeSetId)?.formulas ?? config.default);
  const [activate, setActivate] = useState(false);
  const [slotKey, setSlotKey] = useState<SlotKey>("availability");
  const [errors, setErrors] = useState<Partial<Record<SlotKey, string | null>>>({});
  const [source, setSource] = useState<TestSource>({ mode: "sample", runId: "" });
  const [failure, setFailure] = useState<AppError>();
  const [pending, startTransition] = useTransition();

  const saved = typeof selection === "number" ? config.sets.find((set) => set.id === selection) : undefined;
  const base = saved?.formulas ?? config.default;
  const baseName = saved?.name ?? (selection === "new" ? "" : "Padrão do sistema");
  const readOnly = !config.canManage || selection === "default";
  const isActive = selection === "default" ? config.activeSetId === null : saved?.isActive === true;

  const changedSlots = useMemo(() => config.slots.filter((slot) => formulas[slot.key] !== base[slot.key]).map((slot) => slot.key), [config.slots, formulas, base]);
  const dirty = selection === "new" ? true : !readOnly && (name.trim() !== baseName || changedSlots.length > 0);
  const hasError = Object.values(errors).some(Boolean);

  const slot = config.slots.find((item) => item.key === slotKey) ?? config.slots[0];
  const slotVariables = useMemo(() => config.variables.filter((variable) => variable.slots.includes(slot.key)), [config.variables, slot.key]);

  const onValidity = useCallback((error: string | null) => setErrors((current) => (current[slotKey] === error ? current : { ...current, [slotKey]: error })), [slotKey]);

  function choose(next: Selection, from?: OeeFormulas, fromName?: string) {
    const set = typeof next === "number" ? config.sets.find((item) => item.id === next) : undefined;
    setSelection(next);
    setFormulas(from ?? set?.formulas ?? config.default);
    setName(fromName ?? set?.name ?? (next === "new" ? "" : "Padrão do sistema"));
    setActivate(false);
    setErrors({});
    setFailure(undefined);
  }

  function run(action: () => Promise<{ ok: true } | { ok: false; error: AppError }>, after?: () => void) {
    setFailure(undefined);
    startTransition(async () => {
      const result = await action();
      if (result.ok) after?.();
      else setFailure(result.error);
    });
  }

  function save() {
    const body = { name: name.trim(), formulas, activate };

    if (selection === "new") {
      setFailure(undefined);
      startTransition(async () => {
        const result = await createFormulaSet(body);
        if (result.ok) choose(result.value.id, result.value.formulas, result.value.name);
        else setFailure(result.error);
      });
      return;
    }

    if (typeof selection === "number") run(() => updateFormulaSet(selection, body), () => setActivate(false));
  }

  return (
    <Box sx={{ display: "grid", gap: 2, alignItems: "start", gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "260px minmax(0, 1fr)" } }}>
      <Card sx={{ overflow: "hidden" }}>
        <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center", minHeight: 46, px: 2, py: 0.75, borderBottom: `1px solid ${line}` }}>
          <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
            Conjuntos · {config.sets.length + 1}
          </Typography>
          {config.canManage && (
            <Button size="small" startIcon={<AddOutlined />} onClick={() => choose("new", formulas, "")}>
              Novo
            </Button>
          )}
        </Stack>
        <Box component="ul" sx={{ listStyle: "none", m: 0, p: 0 }}>
          {[{ key: "default" as const, label: "Padrão do sistema", active: config.activeSetId === null, hint: "Fórmulas de fábrica, só leitura" }, ...config.sets.map((set) => ({ key: set.id, label: set.name, active: set.isActive, hint: `Atualizado em ${formatDateTime(set.updatedAt)}` }))].map((item) => (
            <li key={item.key}>
              <ButtonBase
                onClick={() => choose(item.key)}
                aria-current={selection === item.key}
                sx={{
                  display: "block",
                  width: "100%",
                  textAlign: "left",
                  px: 2,
                  py: 1.1,
                  borderTop: `1px solid ${line}`,
                  bgcolor: selection === item.key ? brandSoft : "transparent",
                  boxShadow: selection === item.key ? `inset 3px 0 0 ${brand}` : "none",
                  "&:hover": { bgcolor: brandSoft },
                }}
              >
                <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                  <Typography noWrap sx={{ fontWeight: 600, minWidth: 0 }}>
                    {item.label}
                  </Typography>
                  {item.active && <Chip size="small" color="primary" label="Em uso" />}
                </Stack>
                <Typography variant="caption" sx={{ color: inkSoft }}>
                  {item.hint}
                </Typography>
              </ButtonBase>
            </li>
          ))}
          {selection === "new" && (
            <li>
              <Box sx={{ px: 2, py: 1.1, borderTop: `1px solid ${line}`, bgcolor: brandSoft, boxShadow: `inset 3px 0 0 ${brand}` }}>
                <Typography sx={{ fontWeight: 600 }}>{name.trim() || "Novo conjunto"}</Typography>
                <Typography variant="caption" sx={{ color: inkSoft }}>
                  Ainda não salvo
                </Typography>
              </Box>
            </li>
          )}
        </Box>
      </Card>

      <Card sx={{ p: 2.5 }}>
        <Stack spacing={2.5}>
          <Stack direction={{ xs: "column", md: "row" }} spacing={1.5} sx={{ alignItems: { md: "center" } }}>
            <TextField
              label="Nome do conjunto"
              value={name}
              onChange={(event) => setName(event.target.value)}
              disabled={readOnly}
              size="small"
              required
              sx={{ flex: 1, minWidth: 0 }}
              slotProps={{ htmlInput: { maxLength: 60 } }}
            />
            <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1 }}>
              {selection === "default" && config.canManage && (
                <>
                  <Button variant="outlined" onClick={() => choose("new", config.default, "")}>
                    Criar a partir do padrão
                  </Button>
                  {!isActive && (
                    <Button variant="contained" disabled={pending} onClick={() => run(() => activateFormulaSet(null))}>
                      Voltar ao padrão
                    </Button>
                  )}
                </>
              )}
              {!readOnly && selection === "new" && <FormControlLabel control={<Switch checked={activate} onChange={(event) => setActivate(event.target.checked)} />} label="Usar ao salvar" />}
              {!readOnly && typeof selection === "number" && !isActive && (
                <FormControlLabel control={<Switch checked={activate} onChange={(event) => setActivate(event.target.checked)} />} label="Usar ao salvar" />
              )}
              {!readOnly && (
                <Button variant="contained" disabled={pending || !dirty || hasError || !name.trim()} onClick={save}>
                  {pending ? "Salvando..." : "Salvar"}
                </Button>
              )}
              {!readOnly && typeof selection === "number" && !isActive && (
                <Button variant="outlined" disabled={pending || dirty} onClick={() => run(() => activateFormulaSet(selection))}>
                  Usar este conjunto
                </Button>
              )}
              {!readOnly && saved && !saved.isActive && (
                <ConfirmDeleteButton iconOnly name={`o conjunto ${saved.name}`} action={() => deleteFormulaSet(saved.id).then((result) => { if (result.ok) choose("default"); return result; })} />
              )}
            </Stack>
          </Stack>

          {!config.canManage && (
            <Typography variant="body2" sx={{ color: inkSoft }}>
              Você pode ver e testar as fórmulas, mas só quem gerencia engenharia pode alterá-las.
            </Typography>
          )}

          {failure && <ErrorAlert error={failure} />}

          <Box sx={{ display: "grid", gap: 2.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "210px minmax(0, 1fr)" }, alignItems: "start" }}>
            <Stack spacing={2} component="nav" aria-label="Campos de fórmula">
              {(["post", "line"] as const).map((group) => (
                <Box key={group}>
                  <Typography variant="overline" component="p" sx={{ color: inkSoft, mb: 0.5 }}>
                    {group === "post" ? "Cálculo do posto" : "Cálculo da linha"}
                  </Typography>
                  <Stack spacing={0.5}>
                    {config.slots
                      .filter((item) => item.group === group)
                      .map((item) => {
                        const selected = item.key === slotKey;
                        const changed = changedSlots.includes(item.key);
                        const broken = Boolean(errors[item.key]);
                        return (
                          <ButtonBase
                            key={item.key}
                            onClick={() => setSlotKey(item.key)}
                            aria-current={selected}
                            sx={{
                              display: "flex",
                              alignItems: "center",
                              justifyContent: "space-between",
                              gap: 1,
                              width: "100%",
                              textAlign: "left",
                              px: 1.25,
                              py: 0.9,
                              borderRadius: 1,
                              border: `1px solid ${selected ? brand : line}`,
                              bgcolor: selected ? brandWash : "transparent",
                              "&:hover": { bgcolor: selected ? brandWash : brandSoft },
                            }}
                          >
                            <Typography variant="body2" sx={{ fontWeight: selected ? 700 : 500 }}>
                              {item.label.charAt(0).toUpperCase() + item.label.slice(1)}
                            </Typography>
                            {broken ? <Chip size="small" color="error" label="Erro" /> : changed ? <Chip size="small" variant="outlined" label="Editado" /> : null}
                          </ButtonBase>
                        );
                      })}
                  </Stack>
                </Box>
              ))}
            </Stack>

            <SlotEditor
              key={`${selection}-${slotKey}`}
              slot={slot}
              value={formulas[slot.key]}
              defaultValue={config.default[slot.key]}
              readOnly={readOnly}
              variables={slotVariables}
              functions={config.functions}
              testRuns={testRuns}
              source={source}
              onSource={setSource}
              onChange={(value) => setFormulas((current) => ({ ...current, [slot.key]: value }))}
              onValidity={onValidity}
            />
          </Box>
        </Stack>
      </Card>
    </Box>
  );
}
