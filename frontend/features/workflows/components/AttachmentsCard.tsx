import type { ElementType } from "react";
import AttachFile from "@mui/icons-material/AttachFile";
import Description from "@mui/icons-material/Description";
import FolderZip from "@mui/icons-material/FolderZip";
import Image from "@mui/icons-material/Image";
import PictureAsPdf from "@mui/icons-material/PictureAsPdf";
import TableChart from "@mui/icons-material/TableChart";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import Typography from "@mui/material/Typography";
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
    <Card sx={{ p: 3 }}>
      <Typography variant="overline" component="h2" sx={{ color: inkSoft }}>
        Anexos ({attachments.length})
      </Typography>

      {attachments.length === 0 ? (
        <Typography sx={{ mt: 2, color: inkSoft }}>Este workflow não tem anexos.</Typography>
      ) : (
        <Box sx={{ display: "grid", gap: 2, mt: 2, gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "repeat(2, minmax(0, 1fr))", lg: "repeat(3, minmax(0, 1fr))" } }}>
          {attachments.map((attachment) => {
            const Icon = iconFor(attachment.contentType);

            return (
              <Box key={attachment.id} sx={{ border: `1px solid ${line}`, borderRadius: "12px", p: 2, display: "flex", flexDirection: "column", gap: 1.5 }}>
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
    </Card>
  );
}
