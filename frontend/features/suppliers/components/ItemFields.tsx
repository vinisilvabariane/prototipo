import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import {
  AUDIT_VALIDITIES,
  CRITICALITIES,
  CRITICALITY_LABELS,
  DOCUMENT_KIND_LABELS,
  FIELD_LIMITS,
  QUALIFICATION_STATUSES,
  QUALIFICATION_STATUS_LABELS,
} from "../labels";
import type { SelfAssessment, SupplierAudit, SupplierDocumentKind, SupplierMaterial, SupplierTool, SupplierUser } from "../types";

const limit = (max: number) => ({ htmlInput: { maxLength: max } });
const dateProps = { inputLabel: { shrink: true } };
const triState = (value: boolean | null | undefined) => (value === null || value === undefined ? "" : String(value));

function YesNo({ name, label, value }: { name: string; label: string; value: boolean | null | undefined }) {
  return (
    <TextField select name={name} label={label} defaultValue={triState(value)} fullWidth>
      <MenuItem value="">Não informado</MenuItem>
      <MenuItem value="true">Sim</MenuItem>
      <MenuItem value="false">Não</MenuItem>
    </TextField>
  );
}

export function ToolFields({ tool }: { tool?: SupplierTool }) {
  return (
    <>
      <TextField label="Ferramental" name="name" defaultValue={tool?.name} slotProps={limit(FIELD_LIMITS.toolName)} required fullWidth />
      <TextField label="Ativo fixo" name="assetTag" defaultValue={tool?.assetTag} slotProps={limit(FIELD_LIMITS.assetTag)} fullWidth />
    </>
  );
}

export function MaterialFields({ material }: { material?: SupplierMaterial }) {
  return (
    <>
      <TextField label="SKU" name="sku" defaultValue={material?.sku} slotProps={limit(FIELD_LIMITS.sku)} required fullWidth />
      <TextField label="BU" name="businessUnit" defaultValue={material?.businessUnit} slotProps={limit(FIELD_LIMITS.businessUnit)} fullWidth />
      <TextField label="Tipo de material" name="materialType" defaultValue={material?.materialType} slotProps={limit(FIELD_LIMITS.materialType)} fullWidth />
      <TextField select label="Criticidade" name="criticality" defaultValue={material?.criticality ?? ""} fullWidth>
        <MenuItem value="">Não informada</MenuItem>
        {CRITICALITIES.map((criticality) => (
          <MenuItem key={criticality} value={criticality}>
            {CRITICALITY_LABELS[criticality]}
          </MenuItem>
        ))}
      </TextField>
      <YesNo name="hasSapSpecification" label="Tem especificação no SAP" value={material?.hasSapSpecification} />
    </>
  );
}

function Responsible({ users, value }: { users: SupplierUser[]; value?: number }) {
  return (
    <TextField select label="Responsável pela validação" name="responsibleId" defaultValue={value ?? ""} required fullWidth>
      {users.map((user) => (
        <MenuItem key={user.id} value={user.id}>
          {user.name}
          {user.department && ` · ${user.department}`}
        </MenuItem>
      ))}
    </TextField>
  );
}

function StatusAndScore({ status, score }: { status?: string; score?: number | null }) {
  return (
    <>
      <TextField select label="Status" name="status" defaultValue={status ?? ""} required fullWidth>
        {QUALIFICATION_STATUSES.map((item) => (
          <MenuItem key={item} value={item}>
            {QUALIFICATION_STATUS_LABELS[item]}
          </MenuItem>
        ))}
      </TextField>
      <TextField label="Pontuação" name="score" type="number" defaultValue={score ?? ""} slotProps={{ htmlInput: { min: 0, max: 100, step: 0.01 } }} helperText="De 0 a 100." fullWidth />
    </>
  );
}

export function SelfAssessmentFields({ users, assessment }: { users: SupplierUser[]; assessment?: SelfAssessment }) {
  return (
    <>
      <Responsible users={users} value={assessment?.responsibleId} />
      <TextField label="Data da auto avaliação" name="assessedOn" type="date" defaultValue={assessment?.assessedOn} slotProps={dateProps} required fullWidth helperText="Vence 1 ano e 1 dia depois." />
      <StatusAndScore status={assessment?.status} score={assessment?.score} />
      <YesNo name="needsOnSiteAudit" label="Precisa de auditoria in loco" value={assessment?.needsOnSiteAudit} />
      <TextField label="Link do documento editável" name="editableLink" defaultValue={assessment?.editableLink} slotProps={limit(FIELD_LIMITS.link)} fullWidth />
      <TextField label="Registro do manual" name="documentCode" defaultValue={assessment?.documentCode} slotProps={limit(FIELD_LIMITS.code)} fullWidth />
      <TextField label="Revisão" name="revision" type="number" defaultValue={assessment?.revision ?? ""} slotProps={{ htmlInput: { min: 0, max: 999 } }} fullWidth />
    </>
  );
}

export function AuditFields({ users, audit }: { users: SupplierUser[]; audit?: SupplierAudit }) {
  return (
    <>
      <Responsible users={users} value={audit?.responsibleId} />
      <TextField label="Data da auditoria" name="auditedOn" type="date" defaultValue={audit?.auditedOn} slotProps={dateProps} required fullWidth />
      <TextField select label="Vence em" name="validityMonths" defaultValue={audit?.validityMonths ?? 12} required fullWidth>
        {AUDIT_VALIDITIES.map((months) => (
          <MenuItem key={months} value={months}>
            {months} meses
          </MenuItem>
        ))}
      </TextField>
      <StatusAndScore status={audit?.status} score={audit?.score} />
      <TextField label="Registro do checklist" name="checklistCode" defaultValue={audit?.checklistCode} slotProps={limit(FIELD_LIMITS.code)} fullWidth />
      <TextField label="Revisão do checklist" name="checklistRevision" type="number" defaultValue={audit?.checklistRevision ?? ""} slotProps={{ htmlInput: { min: 0, max: 999 } }} fullWidth />
    </>
  );
}

export function DocumentFields({ kinds, ownerId }: { kinds: SupplierDocumentKind[]; ownerId?: number }) {
  return (
    <>
      {kinds.length > 1 ? (
        <TextField select label="Tipo de documento" name="kind" defaultValue={kinds[0]} fullWidth>
          {kinds.map((kind) => (
            <MenuItem key={kind} value={kind}>
              {DOCUMENT_KIND_LABELS[kind]}
            </MenuItem>
          ))}
        </TextField>
      ) : (
        <input type="hidden" name="kind" value={kinds[0]} />
      )}
      {ownerId !== undefined && <input type="hidden" name="ownerId" value={ownerId} />}
      <TextField label="Arquivo (PDF, JPG ou PNG, até 10 MB)" name="file" type="file" slotProps={{ inputLabel: { shrink: true }, htmlInput: { accept: "application/pdf,image/png,image/jpeg" } }} required fullWidth />
      {kinds.includes("certificate") && (
        <>
          <TextField label="Versão" name="version" slotProps={limit(FIELD_LIMITS.version)} fullWidth />
          <TextField label="Vencimento" name="expiresOn" type="date" slotProps={dateProps} fullWidth />
        </>
      )}
    </>
  );
}
