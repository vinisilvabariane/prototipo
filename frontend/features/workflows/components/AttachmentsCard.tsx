import type { ElementType } from "react";
import AttachFile from "@mui/icons-material/AttachFile";
import Description from "@mui/icons-material/Description";
import FolderZip from "@mui/icons-material/FolderZip";
import Image from "@mui/icons-material/Image";
import PictureAsPdf from "@mui/icons-material/PictureAsPdf";
import TableChart from "@mui/icons-material/TableChart";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import EmptyState from "@/components/ui/EmptyState";
import Panel from "@/components/ui/Panel";
import { inkSoft, line } from "@/lib/tokens";
import { formatBytes, formatDateTime } from "../format";
import type { WorkflowAttachment } from "../types";

function iconFor(contentType: string): ElementType {
  const type = contentType.toLowerCase();
  if (type.includes("pdf")) return PictureAsPdf;
  if (type.startsWith("image/")) return Image;
  if (type.includes("zip") || type.includes("rar") || type.includes("compressed")) return FolderZip;
  if (type.includes("sheet") || type.includes("excel") || type.includes("csv")) return TableChart;
  if (type.includes("word") || type.startsWith("text/")) return Description;
  return AttachFile;
}

export default function AttachmentsCard({ workflowId, attachments }: { workflowId: number; attachments: WorkflowAttachment[] }) {
  return (
    <Panel title="Anexos" count={attachments.length}>
      {attachments.length === 0 ? (
        <EmptyState compact title="Sem anexos" hint="Este workflow não tem arquivos anexados." />
      ) : (
        <Box sx={{ display: "grid", gap: 1.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "repeat(2, minmax(0, 1fr))", lg: "repeat(3, minmax(0, 1fr))" } }}>
          {attachments.map((attachment) => {
            const Icon = iconFor(attachment.contentType);

            return (
              <Box key={attachment.id} sx={{ border: `1px solid ${line}`, borderRadius: "12px", p: 1.5, display: "flex", flexDirection: "column", gap: 1.25 }}>
                <Box sx={{ display: "flex", gap: 1.5, alignItems: "center", minWidth: 0 }}>
                  <Icon sx={{ color: inkSoft }} />
                  <Box sx={{ minWidth: 0 }}>
                    <Typography noWrap title={attachment.name} sx={{ fontWeight: 600 }}>
                      {attachment.name}
                    </Typography>
                    <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
                      {formatBytes(attachment.size)} · {formatDateTime(attachment.uploadedAt)}
                    </Typography>
                  </Box>
                </Box>
                <Button href={`/workflows/${workflowId}/attachments/${attachment.id}`} variant="outlined" size="small" download={attachment.name}>
                  Baixar
                </Button>
              </Box>
            );
          })}
        </Box>
      )}
    </Panel>
  );
}
