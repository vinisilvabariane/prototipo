import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import ConfirmDeleteButton from "@/components/ui/ConfirmDeleteButton";
import FormDialog from "@/components/ui/FormDialog";
import { inkSoft, line } from "@/lib/tokens";
import { removeSupplierDocument, uploadDocument } from "../actions/supplierActions";
import { formatBytes, formatDay } from "../format";
import { DOCUMENT_KIND_LABELS } from "../labels";
import type { SupplierDocument, SupplierDocumentKind } from "../types";
import { DocumentFields } from "./ItemFields";

const REPLACING: SupplierDocumentKind[] = ["presentation", "selfAssessmentAttachment", "declaration", "auditReport"];

type Props = {
  supplierId: number;
  documents: SupplierDocument[];
  kinds: SupplierDocumentKind[];
  canEdit: boolean;
  ownerId?: number;
  title?: string;
  trigger?: string;
};

export default function ItemDocuments({ supplierId, documents, kinds, canEdit, ownerId, title, trigger = "Anexar documento" }: Props) {
  const mine = documents.filter((document) => kinds.includes(document.kind) && (ownerId === undefined ? document.ownerId === null : document.ownerId === ownerId));
  const replacing = kinds.filter((kind) => REPLACING.includes(kind));

  if (mine.length === 0 && !canEdit) return null;

  return (
    <Box sx={{ mt: 1.5, pt: 1.5, borderTop: `1px solid ${line}` }}>
      {title && (
        <Typography variant="body2" sx={{ color: inkSoft, fontWeight: 600, mb: 0.5 }}>
          {title}
        </Typography>
      )}
      <Box sx={{ display: "grid", gap: 0.5 }}>
        {mine.map((document) => (
          <Stack key={document.id} direction="row" spacing={1} sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 0.5 }}>
            <Button size="small" href={`/suppliers/${supplierId}/documents/${document.id}`} target="_blank" rel="noopener" sx={{ textTransform: "none", maxWidth: "100%" }}>
              <Box component="span" sx={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                {document.name}
              </Box>
            </Button>
            <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
              {kinds.length > 1 && `${DOCUMENT_KIND_LABELS[document.kind]} · `}
              {document.version && `v${document.version} · `}
              {formatBytes(document.size)}
            </Typography>
            {document.expiresOn && (
              <Chip size="small" variant="outlined" color={document.expired ? "error" : "default"} label={`${document.expired ? "Vencido em" : "Vence em"} ${formatDay(document.expiresOn)}`} />
            )}
            {canEdit && <ConfirmDeleteButton name={document.name} action={removeSupplierDocument.bind(null, supplierId, document.id)} />}
          </Stack>
        ))}
        {mine.length === 0 && (
          <Typography variant="body2" sx={{ color: inkSoft }}>
            Nenhum documento anexado.
          </Typography>
        )}
      </Box>
      {canEdit && (
        <Box sx={{ mt: 1 }}>
          <FormDialog compact variant="outlined" title={trigger} trigger={trigger} submitLabel="Enviar" action={uploadDocument.bind(null, supplierId)}>
            <DocumentFields kinds={kinds} ownerId={ownerId} />
            {replacing.length > 0 && (
              <Typography variant="body2" sx={{ color: inkSoft }}>
                Enviar de novo substitui o arquivo anterior ({replacing.map((kind) => DOCUMENT_KIND_LABELS[kind].toLowerCase()).join(", ")}).
              </Typography>
            )}
          </FormDialog>
        </Box>
      )}
    </Box>
  );
}
