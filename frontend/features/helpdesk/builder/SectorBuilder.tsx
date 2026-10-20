"use client";

import { useMemo, useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Snackbar from "@mui/material/Snackbar";
import Stack from "@mui/material/Stack";
import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import type { AppError } from "@/lib/result";
import { inkSoft } from "@/lib/tokens";
import { createSector, updateSector } from "../actions/sectorActions";
import type { FieldTemplate, FormField, HelpdeskRole, Location, SaveSectorBody, SectorDetail } from "../types";
import FormEditor from "./FormEditor";
import IdentityTab, { type Identity } from "./IdentityTab";
import SummaryAccessTab from "./SummaryAccessTab";

type Props = {
  sector: SectorDetail | null;
  templates: FieldTemplate[];
  roles: HelpdeskRole[];
  locations: Location[];
};

type StepKey = "identity" | "form" | "resolution" | "access";

const BLANK: Identity = { name: "", description: "", colorHex: "#2547D0", icon: "support", badges: [], active: true, external: false, externalUrl: "" };

function identityOf(sector: SectorDetail | null): Identity {
  if (!sector) return BLANK;

  return {
    name: sector.name,
    description: sector.description ?? "",
    colorHex: sector.colorHex,
    icon: sector.icon,
    badges: sector.badges,
    active: sector.active,
    external: sector.externalUrl !== null,
    externalUrl: sector.externalUrl ?? "",
  };
}

export default function SectorBuilder({ sector, templates: initialTemplates, roles, locations }: Props) {
  const router = useRouter();
  const [tab, setTab] = useState<StepKey>("identity");
  const [identity, setIdentity] = useState<Identity>(() => identityOf(sector));
  const [fields, setFields] = useState<FormField[]>(sector?.form.fields ?? []);
  const [resolution, setResolution] = useState<FormField[]>(sector?.resolutionForm.fields ?? []);
  const [summary, setSummary] = useState<string[]>(sector?.summaryFields ?? []);
  const [attendants, setAttendants] = useState<number[]>(sector?.attendantRoleIds ?? []);
  const [templates, setTemplates] = useState(initialTemplates);
  const [error, setError] = useState<AppError>();
  const [saved, setSaved] = useState(false);
  const [pending, startTransition] = useTransition();

  const body: SaveSectorBody = useMemo(
    () => ({
      name: identity.name.trim(),
      description: identity.description.trim() || null,
      colorHex: identity.colorHex,
      icon: identity.icon,
      badges: identity.badges,
      active: identity.active,
      externalUrl: identity.external ? identity.externalUrl.trim() : null,
      form: { fields: identity.external ? [] : fields },
      resolutionForm: { fields: identity.external ? [] : resolution },
      summaryFields: identity.external ? [] : summary.filter((id) => fields.some((field) => field.id === id)),
      attendantRoleIds: identity.external ? [] : attendants,
    }),
    [identity, fields, resolution, summary, attendants],
  );

  const baseline = useMemo(() => {
    const initial = identityOf(sector);

    return JSON.stringify({
      name: initial.name,
      description: initial.description || null,
      colorHex: initial.colorHex,
      icon: initial.icon,
      badges: initial.badges,
      active: initial.active,
      externalUrl: initial.external ? initial.externalUrl : null,
      form: { fields: sector?.form.fields ?? [] },
      resolutionForm: { fields: sector?.resolutionForm.fields ?? [] },
      summaryFields: sector?.summaryFields ?? [],
      attendantRoleIds: sector?.attendantRoleIds ?? [],
    });
  }, [sector]);

  const dirty = sector === null || JSON.stringify(body) !== baseline;

  function save() {
    setError(undefined);

    startTransition(async () => {
      if (sector === null) {
        const result = await createSector(body);

        if (!result.ok) {
          setError(result.error);
          return;
        }

        router.replace(`/helpdesk/sectors/${result.value.id}`);
        return;
      }

      const result = await updateSector(sector.id, body);

      if (!result.ok) {
        setError(result.error);
        return;
      }

      setSaved(true);
      router.refresh();
    });
  }

  const external = identity.external;

  return (
    <Stack spacing={2.5}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ justifyContent: "space-between", alignItems: { sm: "center" } }}>
        <Tabs value={external && tab !== "identity" ? "identity" : tab} onChange={(_event, next: StepKey) => setTab(next)} variant="scrollable" scrollButtons={false} aria-label="Etapas do setor" sx={{ minHeight: 40, "& .MuiTab-root": { minHeight: 40, py: 0.5 } }}>
          <Tab value="identity" label="Identidade" />
          <Tab value="form" label={`Formulário · ${fields.length}`} disabled={external} />
          <Tab value="resolution" label={`Resposta do técnico · ${resolution.length}`} disabled={external} />
          <Tab value="access" label="Resumo e atendimento" disabled={external} />
        </Tabs>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
          {dirty && sector !== null && (
            <Typography variant="body2" sx={{ color: inkSoft }}>
              Alterações não salvas
            </Typography>
          )}
          <Button href="/helpdesk/sectors" color="inherit">
            Voltar
          </Button>
          <Button variant="contained" onClick={save} disabled={pending || !dirty || !identity.name.trim()}>
            {pending ? "Salvando..." : sector === null ? "Criar setor" : "Salvar setor"}
          </Button>
        </Stack>
      </Stack>

      {error && <ErrorAlert error={error} />}

      <Box>
        {tab === "identity" || external ? (
          <IdentityTab value={identity} onChange={setIdentity} fieldCount={fields.length} />
        ) : tab === "form" ? (
          <FormEditor
            fields={fields}
            onChange={setFields}
            templates={templates}
            onTemplates={setTemplates}
            locations={locations}
            intro="Monte o formulário que quem abre o chamado preenche. Listas e cards podem abrir perguntas novas conforme a opção escolhida: use “+ Pergunta” em cada opção."
          />
        ) : tab === "resolution" ? (
          <FormEditor
            fields={resolution}
            onChange={setResolution}
            resolution
            templates={templates}
            onTemplates={setTemplates}
            locations={locations}
            intro="Formulário que o técnico preenche ao finalizar o chamado. Deixe vazio se o setor não precisa de resposta estruturada. Localização, parada e defeitos ficam só no formulário de abertura."
          />
        ) : (
          <SummaryAccessTab fields={fields} summaryFields={summary} onSummary={setSummary} roles={roles} attendantRoleIds={attendants} onRoles={setAttendants} />
        )}
      </Box>

      <Snackbar open={saved} autoHideDuration={3000} onClose={() => setSaved(false)} message="Setor salvo" />
    </Stack>
  );
}
