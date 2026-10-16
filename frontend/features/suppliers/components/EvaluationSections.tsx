import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Link from "@mui/material/Link";
import Stack from "@mui/material/Stack";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import FormDialog from "@/components/ui/FormDialog";
import { removeSupplierItem, saveAudit, saveSelfAssessment } from "../actions/supplierActions";
import { formatDay, formatScore } from "../format";
import { QUALIFICATION_KIND_LABELS } from "../labels";
import type { SupplierDetail, SupplierUser } from "../types";
import ItemDocuments from "./ItemDocuments";
import { AuditFields, SelfAssessmentFields } from "./ItemFields";
import { Empty, Field, ItemBox, ItemList, Section } from "./Parts";
import { StatusChip } from "./SupplierBadges";

type Props = { supplier: SupplierDetail; users: SupplierUser[] };

export function QualificationSection({ supplier }: { supplier: SupplierDetail }) {
  const current = supplier.qualification;

  return (
    <Section title="Qualificação vigente">
      {!current ? (
        <Empty>Este fornecedor ainda não foi avaliado. Registre uma auto avaliação ou uma auditoria.</Empty>
      ) : (
        <Stack spacing={1.5}>
          {current.auditPending && !current.expired && (
            <Alert severity="warning">A auto avaliação pede uma auditoria in loco que ainda não foi registrada.</Alert>
          )}
          <Box>
            <Field label="Origem">{QUALIFICATION_KIND_LABELS[current.kind]}</Field>
            <Field label="Status">
              <StatusChip status={current.status} expired={current.expired} />
            </Field>
            <Field label="Pontuação">{formatScore(current.score)}</Field>
            <Field label="Realizada em">{formatDay(current.performedOn)}</Field>
            <Field label="Vence em">{formatDay(current.expiresOn)}</Field>
          </Box>
        </Stack>
      )}
    </Section>
  );
}

export function SelfAssessmentsSection({ supplier, users }: Props) {
  const { id, canEdit } = supplier;

  return (
    <Section
      title="Auto avaliações"
      count={supplier.selfAssessments.length}
      action={
        canEdit && (
          <FormDialog compact variant="outlined" maxWidth="sm" title="Registrar auto avaliação" trigger="Registrar" action={saveSelfAssessment.bind(null, id, null)}>
            <SelfAssessmentFields users={users} />
          </FormDialog>
        )
      }
    >
      {supplier.selfAssessments.length === 0 ? (
        <Empty>Nenhuma auto avaliação registrada.</Empty>
      ) : (
        <ItemList>
          {supplier.selfAssessments.map((item) => (
            <ItemBox
              key={item.id}
              title={
                <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 0.5 }}>
                  <span>{formatDay(item.assessedOn)}</span>
                  <StatusChip status={item.status} expired={item.expired} />
                </Stack>
              }
              meta={[
                `Pontuação ${formatScore(item.score)}`,
                `vence em ${formatDay(item.expiresOn)}`,
                `responsável ${item.responsibleName}`,
                item.documentCode && `manual ${item.documentCode}${item.revision !== null ? ` rev. ${item.revision}` : ""}`,
                item.needsOnSiteAudit && "pede auditoria in loco",
              ]
                .filter(Boolean)
                .join(" · ")}
              actions={
                canEdit && (
                  <>
                    <FormDialog compact maxWidth="sm" title="Editar auto avaliação" trigger="Editar" action={saveSelfAssessment.bind(null, id, item.id)}>
                      <SelfAssessmentFields users={users} assessment={item} />
                    </FormDialog>
                    <ConfirmDeleteButton name="esta auto avaliação" action={removeSupplierItem.bind(null, id, "self-assessments", item.id)} />
                  </>
                )
              }
            >
              {item.editableLink && (
                <Box sx={{ mt: 1 }}>
                  <Link href={item.editableLink} target="_blank" rel="noopener noreferrer" sx={{ overflowWrap: "anywhere" }}>
                    Documento editável
                  </Link>
                </Box>
              )}
              <ItemDocuments supplierId={id} documents={supplier.documents} kinds={["selfAssessmentAttachment", "declaration"]} ownerId={item.id} canEdit={canEdit} title="Anexos" />
            </ItemBox>
          ))}
        </ItemList>
      )}
    </Section>
  );
}

export function AuditsSection({ supplier, users }: Props) {
  const { id, canEdit } = supplier;

  return (
    <Section
      title="Auditorias"
      count={supplier.audits.length}
      action={
        canEdit && (
          <FormDialog compact variant="outlined" maxWidth="sm" title="Registrar auditoria" trigger="Registrar" action={saveAudit.bind(null, id, null)}>
            <AuditFields users={users} />
          </FormDialog>
        )
      }
    >
      {supplier.audits.length === 0 ? (
        <Empty>Nenhuma auditoria registrada.</Empty>
      ) : (
        <ItemList>
          {supplier.audits.map((item) => (
            <ItemBox
              key={item.id}
              title={
                <Stack direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 0.5 }}>
                  <span>{formatDay(item.auditedOn)}</span>
                  <StatusChip status={item.status} expired={item.expired} />
                </Stack>
              }
              meta={[
                `Pontuação ${formatScore(item.score)}`,
                `vence em ${formatDay(item.expiresOn)} (${item.validityMonths} meses)`,
                `responsável ${item.responsibleName}`,
                item.checklistCode && `checklist ${item.checklistCode}${item.checklistRevision !== null ? ` rev. ${item.checklistRevision}` : ""}`,
              ]
                .filter(Boolean)
                .join(" · ")}
              actions={
                canEdit && (
                  <>
                    <FormDialog compact maxWidth="sm" title="Editar auditoria" trigger="Editar" action={saveAudit.bind(null, id, item.id)}>
                      <AuditFields users={users} audit={item} />
                    </FormDialog>
                    <ConfirmDeleteButton name="esta auditoria" action={removeSupplierItem.bind(null, id, "audits", item.id)} />
                  </>
                )
              }
            >
              <ItemDocuments supplierId={id} documents={supplier.documents} kinds={["auditReport"]} ownerId={item.id} canEdit={canEdit} title="Relatório" trigger="Anexar relatório" />
            </ItemBox>
          ))}
        </ItemList>
      )}
    </Section>
  );
}
