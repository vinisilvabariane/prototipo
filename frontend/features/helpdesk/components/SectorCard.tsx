import OpenInNew from "@mui/icons-material/OpenInNew";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardActionArea from "@mui/material/CardActionArea";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft } from "@/lib/tokens";
import { HelpdeskIcon } from "../icons";
import type { SectorSummary } from "../types";

export default function SectorCard({ sector }: { sector: SectorSummary }) {
  const external = sector.externalUrl !== null;
  const href = external ? sector.externalUrl! : `/helpdesk/new/${sector.id}`;

  return (
    <Card sx={{ overflow: "hidden", borderTop: `4px solid ${sector.colorHex}`, opacity: sector.active ? 1 : 0.6 }}>
      <CardActionArea
        href={href}
        {...(external ? { target: "_blank", rel: "noopener noreferrer" } : {})}
        sx={{ p: 2.5, height: "100%", display: "flex", flexDirection: "column", alignItems: "flex-start", gap: 1.5 }}
      >
        <Stack direction="row" spacing={1.5} sx={{ alignItems: "center", width: "100%" }}>
          <Box sx={{ width: 44, height: 44, borderRadius: 1.5, display: "grid", placeItems: "center", color: "#fff", bgcolor: sector.colorHex, flexShrink: 0 }}>
            <HelpdeskIcon name={sector.icon} />
          </Box>
          <Typography variant="h6" component="h3" sx={{ flex: 1, minWidth: 0, overflowWrap: "anywhere" }}>
            {sector.name}
          </Typography>
          {external && <OpenInNew fontSize="small" sx={{ color: inkSoft }} titleAccess="Abre em outra aba" />}
        </Stack>
        {sector.description && (
          <Typography variant="body2" sx={{ color: inkSoft }}>
            {sector.description}
          </Typography>
        )}
        <Stack direction="row" sx={{ flexWrap: "wrap", gap: 0.75, mt: "auto" }}>
          {!sector.active && <Chip size="small" label="Desativado" />}
          {sector.badges.map((badge) => (
            <Chip key={badge} size="small" variant="outlined" label={badge} />
          ))}
        </Stack>
      </CardActionArea>
    </Card>
  );
}
