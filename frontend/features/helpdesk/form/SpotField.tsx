"use client";

import Box from "@mui/material/Box";
import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { inkSoft } from "@/lib/tokens";
import { POST_SUFFIX } from "../schema";
import type { Answers, FormField, Location } from "../types";

type Props = { field: FormField; answers: Answers; locations: Location[]; onChange: (patch: Answers) => void; disabled?: boolean };

export default function SpotField({ field, answers, locations, onChange, disabled }: Props) {
  const lineId = answers[field.id] ?? "";
  const postId = answers[field.id + POST_SUFFIX] ?? "";
  const line = locations.find((location) => String(location.lineId) === lineId);

  return (
    <Box>
      <Box sx={{ display: "grid", gap: 1.5, gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "repeat(2, minmax(0, 1fr))" } }}>
        <TextField
          select
          label="Linha"
          value={lineId}
          required={field.required}
          disabled={disabled}
          onChange={(event) => onChange({ [field.id]: event.target.value, [field.id + POST_SUFFIX]: "" })}
          fullWidth
        >
          {locations.map((location) => (
            <MenuItem key={location.lineId} value={String(location.lineId)}>
              {location.lineName}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Posto"
          value={postId}
          disabled={disabled || !line || line.posts.length === 0}
          onChange={(event) => onChange({ [field.id + POST_SUFFIX]: event.target.value })}
          fullWidth
        >
          <MenuItem value="">Linha toda / não sei</MenuItem>
          {line?.posts.map((post) => (
            <MenuItem key={post.id} value={String(post.id)}>
              {post.name}
            </MenuItem>
          ))}
        </TextField>
      </Box>
      {line && line.posts.length === 0 && (
        <Typography variant="caption" sx={{ color: inkSoft, display: "block", mt: 0.5 }}>
          Essa linha não tem produção em andamento, então não há postos para escolher.
        </Typography>
      )}
    </Box>
  );
}
