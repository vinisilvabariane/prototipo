"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import CheckCircleOutlined from "@mui/icons-material/CheckCircleOutlined";
import ErrorOutlined from "@mui/icons-material/ErrorOutlined";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import Typography from "@mui/material/Typography";
import { brandSoft, brandWash, inkSoft, line } from "@/lib/tokens";
import { testFormula } from "../actions/formulaActions";
import type { FormulaTest, OeeFunction, OeeSlot, OeeVariable } from "../types";
import VariablePalette from "./VariablePalette";

export type TestRun = { id: number; label: string };

export type TestSource = { mode: "sample" | "run"; runId: number | "" };

type Props = {
  slot: OeeSlot;
  value: string;
  defaultValue: string;
  readOnly: boolean;
  variables: OeeVariable[];
  functions: OeeFunction[];
  testRuns: TestRun[];
  source: TestSource;
  onSource: (source: TestSource) => void;
  onChange: (value: string) => void;
  onValidity: (error: string | null) => void;
};

const OPERATORS = [
  { label: "+", text: " + " },
  { label: "−", text: " - " },
  { label: "×", text: " * " },
  { label: "÷", text: " / " },
  { label: "(", text: "(" },
  { label: ")", text: ")" },
  { label: ",", text: ", " },
  { label: "<", text: " < " },
  { label: ">", text: " > " },
  { label: "≤", text: " <= " },
  { label: "≥", text: " >= " },
  { label: "=", text: " == " },
  { label: "≠", text: " != " },
];

const SAMPLE_VALUES: Record<string, number> = {
  elapsed_seconds: 3600,
  oee_time_rate_seconds: 3600,
  occupied_seconds: 3600,
  theoretical_production: 100,
  max_rate_hour: 100,
  expected_production: 100,
  expected_production_min: 90,
  real_production: 80,
  line_plan: 100,
  line_produced: 80,
  line_produced_station: 80,
  line_elapsed: 3600,
  availability: 1,
  performance: 0.8,
  quality: 1,
  machine_oee: 0.8,
};

const sampleOf = (key: string) => (key.includes("_average") || key.includes("_last_post") || key.includes("_product") ? 0.9 : (SAMPLE_VALUES[key] ?? 0));

function Marker({ text, position }: { text: string; position: number }) {
  const at = Math.min(Math.max(position, 0), text.length);
  const end = at < text.length ? at + 1 : at;

  return (
    <Box sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere", fontWeight: 600, fontSize: 14, bgcolor: brandSoft, border: `1px solid ${brandWash}`, borderRadius: 1, px: 1.25, py: 0.75 }}>
      {text.slice(0, at)}
      <Box component="mark" sx={{ bgcolor: "error.main", color: "error.contrastText", borderRadius: "3px", px: "3px", minWidth: 6, display: "inline-block" }}>
        {text.slice(at, end) || " "}
      </Box>
      {text.slice(end)}
    </Box>
  );
}

export default function SlotEditor({ slot, value, defaultValue, readOnly, variables, functions, testRuns, source, onSource, onChange, onValidity }: Props) {
  const { mode, runId } = source;
  const input = useRef<HTMLTextAreaElement | null>(null);
  const [manual, setManual] = useState<Record<string, string>>({});
  const [test, setTest] = useState<FormulaTest | null>(null);
  const [checking, setChecking] = useState(false);
  const [failed, setFailed] = useState<string | null>(null);

  const manualValues = useMemo(() => {
    const values: Record<string, number> = {};
    for (const key of test?.identifiers ?? []) {
      const typed = manual[key];
      values[key] = typed !== undefined && typed !== "" && Number.isFinite(Number(typed.replace(",", "."))) ? Number(typed.replace(",", ".")) : sampleOf(key);
    }
    return values;
  }, [manual, test?.identifiers]);

  const manualKey = JSON.stringify(manualValues);

  useEffect(() => {
    const body = { slot: slot.key, expression: value, ...(mode === "run" && runId !== "" ? { runId } : { values: JSON.parse(manualKey) as Record<string, number> }) };
    let cancelled = false;

    const timer = setTimeout(async () => {
      setChecking(true);
      const result = await testFormula(body);
      if (cancelled) return;
      setChecking(false);

      if (!result.ok) {
        setFailed(result.error.messages[0] ?? result.error.title);
        return;
      }

      setFailed(null);
      setTest(result.value);
      onValidity(result.value.valid ? null : result.value.error);
    }, 350);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [slot.key, value, mode, runId, manualKey, onValidity]);

  function insert(text: string, caretBack = 0) {
    const field = input.current;
    const start = field?.selectionStart ?? value.length;
    const end = field?.selectionEnd ?? value.length;
    onChange(`${value.slice(0, start)}${text}${value.slice(end)}`);

    requestAnimationFrame(() => {
      if (!field) return;
      const caret = start + text.length - caretBack;
      field.focus();
      field.setSelectionRange(caret, caret);
    });
  }

  const invalid = test !== null && !test.valid;
  const result = test?.valid && test.result !== null ? test.result : null;

  return (
    <Stack spacing={2}>
      <Box>
        <TextField
          label={`Fórmula de ${slot.label}`}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          multiline
          minRows={3}
          fullWidth
          disabled={readOnly}
          error={invalid}
          inputRef={input}
          slotProps={{ htmlInput: { spellCheck: false, autoCapitalize: "off", autoComplete: "off", style: { fontSize: 15, lineHeight: 1.6 } } }}
        />
        <Stack direction="row" spacing={1} sx={{ mt: 0.75, alignItems: "center", minHeight: 28 }} aria-live="polite">
          {invalid ? (
            <>
              <ErrorOutlined color="error" sx={{ fontSize: 18 }} />
              <Typography variant="body2" color="error">
                {test.error}
              </Typography>
            </>
          ) : result !== null ? (
            <>
              <CheckCircleOutlined color="success" sx={{ fontSize: 18 }} />
              <Typography variant="body2" sx={{ color: inkSoft }}>
                Fórmula válida{checking ? " · conferindo…" : ""}
              </Typography>
            </>
          ) : (
            <Typography variant="body2" sx={{ color: inkSoft }}>
              {checking ? "Conferindo…" : "Escreva a fórmula usando as variáveis ao lado."}
            </Typography>
          )}
          {!readOnly && value !== defaultValue && (
            <Button size="small" sx={{ ml: "auto" }} onClick={() => onChange(defaultValue)}>
              Voltar ao padrão deste campo
            </Button>
          )}
        </Stack>
        {invalid && test.position !== null && value.trim() && <Marker text={value} position={test.position} />}
        {failed && (
          <Typography variant="body2" color="error" sx={{ mt: 0.5 }}>
            {failed}
          </Typography>
        )}
      </Box>

      {!readOnly && (
        <Stack spacing={1}>
          <Stack direction="row" sx={{ flexWrap: "wrap", gap: 0.75 }} aria-label="Operadores">
            {OPERATORS.map((operator) => (
              <Button key={operator.label} size="small" variant="outlined" onClick={() => insert(operator.text)} sx={{ minWidth: 36, px: 1, fontWeight: 700 }} aria-label={`Inserir ${operator.label}`}>
                {operator.label}
              </Button>
            ))}
          </Stack>
          <Stack direction="row" sx={{ flexWrap: "wrap", gap: 0.75 }} aria-label="Funções">
            {functions.map((fn) => (
              <Chip key={fn.name} size="small" variant="outlined" clickable label={fn.signature} title={fn.description} onClick={() => insert(`${fn.name}()`, 1)} />
            ))}
          </Stack>
        </Stack>
      )}

      <VariablePalette variables={variables} disabled={readOnly} onPick={(key) => insert(key)} />

      <Box sx={{ border: `1px solid ${line}`, borderRadius: 1, p: 1.5 }}>
        <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ justifyContent: "space-between", alignItems: { sm: "center" }, mb: 1.25 }}>
          <Typography variant="overline" component="h3" sx={{ color: inkSoft }}>
            Testar a fórmula
          </Typography>
          <ToggleButtonGroup exclusive size="small" value={mode} onChange={(_event, next: "sample" | "run" | null) => next && onSource({ ...source, mode: next })} aria-label="Fonte dos valores do teste">
            <ToggleButton value="sample" sx={{ textTransform: "none", px: 1.5 }}>
              Valores de exemplo
            </ToggleButton>
            <ToggleButton value="run" sx={{ textTransform: "none", px: 1.5 }} disabled={testRuns.length === 0}>
              Produção real
            </ToggleButton>
          </ToggleButtonGroup>
        </Stack>

        {mode === "run" ? (
          <TextField select size="small" label="Produção" value={runId} onChange={(event) => onSource({ ...source, runId: event.target.value === "" ? "" : Number(event.target.value) })} fullWidth sx={{ mb: 1.25 }}>
            {testRuns.map((run) => (
              <MenuItem key={run.id} value={run.id}>
                {run.label}
              </MenuItem>
            ))}
          </TextField>
        ) : (
          test &&
          test.identifiers.length > 0 && (
            <Box sx={{ display: "grid", gap: 1, gridTemplateColumns: { xs: "1fr", sm: "repeat(2, minmax(0, 1fr))" }, mb: 1.25 }}>
              {test.identifiers.map((key) => (
                <TextField
                  key={key}
                  size="small"
                  label={variables.find((variable) => variable.key === key)?.label ?? key}
                  value={manual[key] ?? String(sampleOf(key)).replace(".", ",")}
                  onChange={(event) => setManual((current) => ({ ...current, [key]: event.target.value }))}
                  slotProps={{ htmlInput: { inputMode: "decimal" } }}
                />
              ))}
            </Box>
          )
        )}

        <Stack direction="row" spacing={1.5} sx={{ alignItems: "baseline" }}>
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Resultado
          </Typography>
          <Typography sx={{ fontSize: 22, fontWeight: 700, fontVariantNumeric: "tabular-nums" }}>{result === null ? "—" : result.toLocaleString("pt-BR", { maximumFractionDigits: 4 })}</Typography>
          {result !== null && (
            <Typography variant="body2" sx={{ color: inkSoft }}>
              {(result * 100).toLocaleString("pt-BR", { maximumFractionDigits: 2 })}% se for lido como razão
            </Typography>
          )}
        </Stack>
      </Box>
    </Stack>
  );
}
