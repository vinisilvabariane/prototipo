import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import {
  CRITICALITIES,
  CRITICALITY_LABELS,
  MULTI_BRANCHES,
  MULTI_BRANCH_LABELS,
  ORIGINS,
  ORIGIN_LABELS,
  STATUS_FILTERS,
  STATUS_FILTER_LABELS,
} from "../labels";

export type SupplierFilterValues = {
  search: string;
  origin: string;
  multiBranch: string;
  criticality: string;
  status: string;
};

function Select({ name, label, value, options }: { name: string; label: string; value: string; options: [string, string][] }) {
  return (
    <TextField select name={name} label={label} size="small" defaultValue={value} sx={{ minWidth: 150 }}>
      <MenuItem value="">Todos</MenuItem>
      {options.map(([key, text]) => (
        <MenuItem key={key} value={key}>
          {text}
        </MenuItem>
      ))}
    </TextField>
  );
}

export default function SupplierFilters({ values }: { values: SupplierFilterValues }) {
  return (
    <Box component="form" action="/suppliers" sx={{ display: "flex", gap: 1, flexWrap: "wrap", alignItems: "center" }}>
      <TextField name="search" label="Buscar por nome, código ou CNPJ" size="small" defaultValue={values.search} sx={{ minWidth: { sm: 300 } }} />
      <Select name="status" label="Status" value={values.status} options={STATUS_FILTERS.map((item) => [item, STATUS_FILTER_LABELS[item]])} />
      <Select name="multiBranch" label="Filial Multi" value={values.multiBranch} options={MULTI_BRANCHES.map((item) => [item, MULTI_BRANCH_LABELS[item]])} />
      <Select name="criticality" label="Criticidade" value={values.criticality} options={CRITICALITIES.map((item) => [item, CRITICALITY_LABELS[item]])} />
      <Select name="origin" label="Origem" value={values.origin} options={ORIGINS.map((item) => [item, ORIGIN_LABELS[item]])} />
      <Button type="submit" variant="outlined">
        Filtrar
      </Button>
      <Button href="/suppliers">Limpar</Button>
    </Box>
  );
}
