import type { ElementType } from "react";
import PersonAddAlt1Outlined from "@mui/icons-material/PersonAddAlt1Outlined";
import PriorityHigh from "@mui/icons-material/PriorityHigh";
import PrecisionManufacturing from "@mui/icons-material/PrecisionManufacturing";
import VerifiedUserOutlined from "@mui/icons-material/VerifiedUserOutlined";
import PersonOutlined from "@mui/icons-material/PersonOutlined";
import Box from "@mui/material/Box";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import { brand, brandWash, ink, inkSoft, line, lineStrong, surface } from "@/lib/tokens";
import { POST_KIND_LABELS } from "../labels";
import { formatQuantity } from "../format";
import type { PostKind } from "../types";
import type { PostNodeData } from "./nodeData";

const KIND_ICON: Record<PostKind, ElementType> = {
  standard: PersonOutlined,
  machine: PrecisionManufacturing,
  quality: VerifiedUserOutlined,
};

const KIND_ACCENT: Record<PostKind, string> = {
  standard: lineStrong,
  machine: brand,
  quality: "var(--mui-palette-success-main)",
};

function initials(name: string) {
  const parts = name.trim().split(/\s+/);
  return ((parts[0]?.[0] ?? "") + (parts.length > 1 ? (parts[parts.length - 1][0] ?? "") : "")).toUpperCase();
}

function attainmentColor(value: number | null) {
  if (value === null) return lineStrong;
  if (value >= 1) return "var(--mui-palette-success-main)";
  if (value >= 0.8) return brand;
  return "var(--mui-palette-warning-main)";
}

type Props = { data: PostNodeData; selected: boolean; zoom: number };

export default function PostCard({ data, selected, zoom }: Props) {
  const Icon = KIND_ICON[data.kind];
  const accent = KIND_ACCENT[data.kind];
  const compact = zoom < 0.45;
  const emptyCritical = data.isCritical && !data.holder;

  return (
    <Box
      sx={{
        position: "relative",
        width: "100%",
        height: "100%",
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
        borderRadius: "10px",
        bgcolor: surface,
        border: `1.5px solid ${data.invalid ? "var(--mui-palette-error-main)" : selected ? brand : line}`,
        boxShadow: selected ? `0 0 0 3px ${brandWash}` : "0 1px 2px rgb(0 0 0 / 6%)",
        transform: data.rotation ? `rotate(${data.rotation}deg)` : undefined,
        transformOrigin: "center",
        transition: "box-shadow 120ms, border-color 120ms",
        "&::before": { content: '""', position: "absolute", insetBlock: 0, left: 0, width: 5, bgcolor: accent },
      }}
    >
      {compact ? (
        <Typography sx={{ m: "auto", px: 2, textAlign: "center", fontWeight: 700, fontSize: 34, lineHeight: 1.1, color: ink }}>{data.name}</Typography>
      ) : (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 0.75, flex: 1, minHeight: 0, pl: 2, pr: 1.25, py: 1.1 }}>
          <Box sx={{ display: "flex", alignItems: "center", gap: 0.75, minWidth: 0 }}>
            <Tooltip title={POST_KIND_LABELS[data.kind]}>
              <Icon sx={{ fontSize: 16, color: data.kind === "standard" ? inkSoft : accent, flexShrink: 0 }} />
            </Tooltip>
            <Typography noWrap sx={{ flex: 1, minWidth: 0, fontWeight: 700, fontSize: 14, color: ink }}>
              {data.name || "Sem nome"}
            </Typography>
            {data.isCritical && (
              <Tooltip title="Posto crítico: precisa de alguém para iniciar a produção">
                <Box sx={{ display: "grid", placeItems: "center", width: 18, height: 18, borderRadius: "50%", flexShrink: 0, color: "var(--mui-palette-error-contrastText)", bgcolor: "var(--mui-palette-error-main)" }}>
                  <PriorityHigh sx={{ fontSize: 13 }} />
                </Box>
              </Tooltip>
            )}
          </Box>

          {data.holder ? (
            <Box sx={{ display: "flex", alignItems: "center", gap: 1, minWidth: 0 }}>
              <Box sx={{ display: "grid", placeItems: "center", width: 28, height: 28, borderRadius: "50%", flexShrink: 0, bgcolor: brandWash, color: brand, fontSize: 11, fontWeight: 700 }}>
                {initials(data.holder.name)}
              </Box>
              <Box sx={{ minWidth: 0 }}>
                <Typography noWrap sx={{ fontSize: 13, fontWeight: 600, color: ink, lineHeight: 1.2 }}>
                  {data.holder.name}
                </Typography>
                <Typography noWrap sx={{ fontSize: 11, color: inkSoft, lineHeight: 1.2, fontVariantNumeric: "tabular-nums" }}>
                  Mat. {data.holder.registration}
                </Typography>
              </Box>
            </Box>
          ) : data.editable ? (
            <Typography noWrap sx={{ fontSize: 12, color: inkSoft }}>
              {data.operation || "Sem operação descrita"}
            </Typography>
          ) : (
            <Box sx={{ display: "flex", alignItems: "center", gap: 0.75, px: 1, py: 0.6, borderRadius: "8px", border: `1.5px dashed ${emptyCritical ? "var(--mui-palette-error-main)" : lineStrong}`, color: emptyCritical ? "var(--mui-palette-error-main)" : inkSoft }}>
              <PersonAddAlt1Outlined sx={{ fontSize: 16 }} />
              <Typography noWrap sx={{ fontSize: 12, fontWeight: 600 }}>
                {emptyCritical ? "Crítico sem ninguém" : "Sem ninguém"}
              </Typography>
            </Box>
          )}

          <Box sx={{ mt: "auto", minHeight: 0 }}>
            {data.showMetrics ? (
              <Box>
                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "baseline", gap: 1 }}>
                  <Typography sx={{ fontSize: 12, color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
                    <Box component="strong" sx={{ color: ink, fontSize: 15 }}>
                      {formatQuantity(data.produced)}
                    </Box>
                    {data.expected !== null && ` de ${formatQuantity(data.expected)}`}
                  </Typography>
                  {data.attainment !== null && (
                    <Typography sx={{ fontSize: 12, fontWeight: 700, color: attainmentColor(data.attainment), fontVariantNumeric: "tabular-nums" }}>
                      {Math.round(data.attainment * 100)}%
                    </Typography>
                  )}
                </Box>
                <Box sx={{ mt: 0.5, height: 4, borderRadius: 2, bgcolor: line, overflow: "hidden" }}>
                  <Box sx={{ height: "100%", width: `${Math.min(100, Math.round((data.attainment ?? 0) * 100))}%`, bgcolor: attainmentColor(data.attainment) }} />
                </Box>
              </Box>
            ) : (
              data.targetPerHour !== null && (
                <Typography sx={{ fontSize: 11, color: inkSoft, fontVariantNumeric: "tabular-nums" }}>Meta {formatQuantity(data.targetPerHour)} por hora</Typography>
              )
            )}
          </Box>
        </Box>
      )}
    </Box>
  );
}
