"use client";

import type { ReactNode } from "react";
import CheckCircleOutlined from "@mui/icons-material/CheckCircleOutlined";
import ErrorOutlined from "@mui/icons-material/ErrorOutlined";
import InfoOutlined from "@mui/icons-material/InfoOutlined";
import WarningAmber from "@mui/icons-material/WarningAmber";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import ToggleButton from "@mui/material/ToggleButton";
import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import Typography from "@mui/material/Typography";
import Panel from "@/components/ui/Panel";
import { inkSoft, line } from "@/lib/tokens";
import { DEFAULT_PREFS, resetPrefs, setPref, usePrefs, type Prefs } from "@/lib/prefs/store";

type Choice<T extends string> = { value: T; label: string };

function Section({ title, description, children }: { title: string; description: string; children: ReactNode }) {
  return (
    <Panel title={title}>
      <Typography variant="body2" sx={{ color: inkSoft, mb: 2.5 }}>
        {description}
      </Typography>
      <Stack spacing={2.5}>{children}</Stack>
    </Panel>
  );
}

function ChoiceRow<T extends string>({ label, hint, value, options, onChange }: { label: string; hint?: string; value: T; options: Choice<T>[]; onChange: (value: T) => void }) {
  return (
    <Box>
      <Typography component="p" sx={{ fontWeight: 600 }} id={`choice-${label}`}>
        {label}
      </Typography>
      {hint && (
        <Typography variant="body2" sx={{ color: inkSoft, mb: 1 }}>
          {hint}
        </Typography>
      )}
      <ToggleButtonGroup
        exclusive
        size="small"
        value={value}
        onChange={(_, next: T | null) => next && onChange(next)}
        aria-labelledby={`choice-${label}`}
        sx={{ mt: 1, flexWrap: "wrap" }}
      >
        {options.map((option) => (
          <ToggleButton key={option.value} value={option.value} sx={{ px: 2 }}>
            {option.label}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
    </Box>
  );
}

function SwitchRow({ label, hint, checked, onChange }: { label: string; hint: string; checked: boolean; onChange: (checked: boolean) => void }) {
  return (
    <FormControlLabel
      control={<Switch checked={checked} onChange={(_, next) => onChange(next)} />}
      label={
        <Box sx={{ ml: 0.5 }}>
          <Typography sx={{ fontWeight: 600 }}>{label}</Typography>
          <Typography variant="body2" sx={{ color: inkSoft }}>
            {hint}
          </Typography>
        </Box>
      }
      sx={{ alignItems: "flex-start", m: 0 }}
    />
  );
}

const COLOR_SAMPLES = [
  { label: "Erro", color: "error", icon: ErrorOutlined },
  { label: "Aviso", color: "warning", icon: WarningAmber },
  { label: "Sucesso", color: "success", icon: CheckCircleOutlined },
  { label: "Informação", color: "primary", icon: InfoOutlined },
] as const;

function ColorSample() {
  return (
    <Box sx={{ p: 2, border: `1px solid ${line}`, borderRadius: "10px" }}>
      <Typography variant="overline" sx={{ color: inkSoft }}>
        Amostra de cores
      </Typography>
      <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: "wrap", rowGap: 1 }}>
        {COLOR_SAMPLES.map(({ label, color, icon: Icon }) => (
          <Chip key={label} color={color} icon={<Icon />} label={label} />
        ))}
        {COLOR_SAMPLES.map(({ label, color }) => (
          <Chip key={`${label}-outlined`} color={color} variant="outlined" label={label} />
        ))}
      </Stack>
      <Stack spacing={1} sx={{ mt: 1.5 }}>
        <Alert severity="error">Rejeitado: o lote ficou abaixo da meta.</Alert>
        <Alert severity="warning">Atenção: a qualificação vence em 7 dias.</Alert>
        <Alert severity="success">Aprovado: todos os postos críticos estão ocupados.</Alert>
      </Stack>
    </Box>
  );
}

const SAMPLE =
  "A ordem de produção 48213 foi liberada para a linha 2 às 07:40. O lote de 1.200 peças deve ser concluído até o fim do turno, com inspeção final na expedição.";

export default function SettingsForm() {
  const prefs = usePrefs();
  const isDefault = (Object.keys(DEFAULT_PREFS) as (keyof Prefs)[]).every((key) => key === "theme" || prefs[key] === DEFAULT_PREFS[key]);

  return (
    <Stack spacing={2.5}>
      <Section title="Leitura" description="Ajuste o texto para ler com conforto. A amostra abaixo muda na hora.">
        <ChoiceRow
          label="Tamanho do texto"
          value={prefs.fontSize}
          onChange={(next) => setPref("fontSize", next)}
          options={[
            { value: "sm", label: "Pequeno" },
            { value: "md", label: "Normal" },
            { value: "lg", label: "Grande" },
            { value: "xl", label: "Muito grande" },
          ]}
        />
        <ChoiceRow
          label="Espaço entre linhas"
          value={prefs.lineHeight}
          onChange={(next) => setPref("lineHeight", next)}
          options={[
            { value: "normal", label: "Normal" },
            { value: "relaxed", label: "Amplo" },
          ]}
        />
        <Box sx={{ p: 2, border: `1px solid ${line}`, borderRadius: "10px" }}>
          <Typography variant="overline" sx={{ color: inkSoft }}>
            Amostra
          </Typography>
          <Typography sx={{ mt: 0.5 }}>{SAMPLE}</Typography>
        </Box>
      </Section>

      <Section title="Acessibilidade" description="Recursos para quem usa Libras, tem baixa visão ou daltonismo.">
        <SwitchRow
          label="Tradutor de Libras (VLibras)"
          hint="Mostra um botão no canto da tela que traduz o texto para Libras. Usa o serviço público VLibras e precisa de internet."
          checked={prefs.libras}
          onChange={(next) => setPref("libras", next)}
        />
        <SwitchRow
          label="Alto contraste"
          hint="Texto mais escuro, bordas mais fortes e links sublinhados."
          checked={prefs.contrast === "high"}
          onChange={(next) => setPref("contrast", next ? "high" : "normal")}
        />
        <ChoiceRow
          label="Daltonismo"
          hint="Ajusta as cores de erro, aviso e sucesso para o tipo de visão. A amostra abaixo muda na hora; ícones e textos continuam reforçando o significado."
          value={prefs.colorVision}
          onChange={(next) => setPref("colorVision", next)}
          options={[
            { value: "normal", label: "Nenhum" },
            { value: "protanopia", label: "Protanopia" },
            { value: "deuteranopia", label: "Deuteranopia" },
            { value: "tritanopia", label: "Tritanopia" },
            { value: "achromatopsia", label: "Acromatopsia" },
          ]}
        />
        <ColorSample />
        <Typography variant="body2" sx={{ color: inkSoft, mt: -2 }}>
          Protanopia (sem vermelho): laranja claro, amarelo e ciano. Deuteranopia (sem verde): laranja, amarelo e azul-esverdeado. Tritanopia (sem azul): vermelho, ciano e cinza-escuro. Acromatopsia (sem cor): só tons de cinza.
        </Typography>
        <SwitchRow
          label="Reduzir animações"
          hint="Desliga as transições e os efeitos de entrada. O sistema já respeita essa preferência quando ela está ativa no seu dispositivo."
          checked={prefs.motion === "reduced"}
          onChange={(next) => setPref("motion", next ? "reduced" : "full")}
        />
      </Section>

      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 2 }}>
        <Typography variant="body2" sx={{ color: inkSoft }}>
          As preferências ficam salvas neste navegador. O tema é escolhido pelo ícone de paleta na barra.
        </Typography>
        <Button variant="outlined" onClick={resetPrefs} disabled={isDefault}>
          Restaurar padrões
        </Button>
      </Box>
    </Stack>
  );
}
