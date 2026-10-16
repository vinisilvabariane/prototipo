import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import ButtonGroup from "@mui/material/ButtonGroup";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { FILTER_LABELS } from "../labels";
import type { WorkflowFilter } from "../types";

const FILTERS = Object.keys(FILTER_LABELS) as WorkflowFilter[];

function href(filter: WorkflowFilter, search: string) {
  const query = new URLSearchParams();
  if (filter !== "all") query.set("filter", filter);
  if (search) query.set("search", search);
  const text = query.toString();
  return text ? `/workflows?${text}` : "/workflows";
}

export default function WorkflowFilters({ filter, search }: { filter: WorkflowFilter; search: string }) {
  return (
    <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={{ justifyContent: "space-between", alignItems: { md: "center" } }}>
      <ButtonGroup aria-label="Filtro de workflows">
        {FILTERS.map((item) => (
          <Button key={item} href={href(item, search)} variant={item === filter ? "contained" : "outlined"} aria-current={item === filter ? "true" : undefined}>
            {FILTER_LABELS[item]}
          </Button>
        ))}
      </ButtonGroup>

      <Box component="form" action="/workflows" sx={{ display: "flex", gap: 1 }}>
        {filter !== "all" && <input type="hidden" name="filter" value={filter} />}
        <TextField name="search" label="Buscar por título ou descrição" size="small" defaultValue={search} sx={{ minWidth: { md: 300 } }} />
        <Button type="submit" variant="outlined">
          Buscar
        </Button>
      </Box>
    </Stack>
  );
}
