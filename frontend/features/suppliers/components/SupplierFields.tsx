import Box from "@mui/material/Box";
import MenuItem from "@mui/material/MenuItem";
import TextField from "@mui/material/TextField";
import {
  CRITICALITIES,
  CRITICALITY_LABELS,
  FIELD_LIMITS,
  MULTI_BRANCHES,
  MULTI_BRANCH_LABELS,
  ORIGINS,
  ORIGIN_LABELS,
} from "../labels";
import type { SupplierDetail } from "../types";

const limit = (max: number) => ({ htmlInput: { maxLength: max } });

export default function SupplierFields({ supplier }: { supplier?: SupplierDetail }) {
  return (
    <Box sx={{ display: "grid", gap: 2, gridTemplateColumns: { xs: "minmax(0, 1fr)", sm: "repeat(2, minmax(0, 1fr))" } }}>
      <TextField label="Código SAP" name="sapCode" defaultValue={supplier?.sapCode} slotProps={limit(FIELD_LIMITS.sapCode)} required />
      <TextField select label="Filial Multi" name="multiBranch" defaultValue={supplier?.multiBranch ?? ""} required>
        {MULTI_BRANCHES.map((branch) => (
          <MenuItem key={branch} value={branch}>
            {MULTI_BRANCH_LABELS[branch]}
          </MenuItem>
        ))}
      </TextField>
      <TextField label="Nome fantasia" name="name" defaultValue={supplier?.name} slotProps={limit(FIELD_LIMITS.name)} required sx={{ gridColumn: { sm: "1 / -1" } }} />
      <TextField label="Razão social" name="corporateName" defaultValue={supplier?.corporateName} slotProps={limit(FIELD_LIMITS.corporateName)} />
      <TextField label="Filial do fornecedor" name="branch" defaultValue={supplier?.branch} slotProps={limit(FIELD_LIMITS.branch)} required />
      <TextField select label="Origem" name="origin" defaultValue={supplier?.origin ?? ""} required>
        {ORIGINS.map((origin) => (
          <MenuItem key={origin} value={origin}>
            {ORIGIN_LABELS[origin]}
          </MenuItem>
        ))}
      </TextField>
      <TextField label="CNPJ ou documento fiscal" name="taxId" defaultValue={supplier?.taxId} slotProps={limit(FIELD_LIMITS.taxId)} helperText="Fornecedor nacional: CNPJ válido." />
      <TextField select label="Criticidade" name="criticality" defaultValue={supplier?.criticality ?? ""} required>
        {CRITICALITIES.map((criticality) => (
          <MenuItem key={criticality} value={criticality}>
            {CRITICALITY_LABELS[criticality]}
          </MenuItem>
        ))}
      </TextField>
      <TextField label="CEP" name="zipCode" defaultValue={supplier?.zipCode} slotProps={limit(FIELD_LIMITS.zipCode)} required />
      <TextField label="Bairro" name="district" defaultValue={supplier?.district} slotProps={limit(FIELD_LIMITS.district)} required />
      <TextField label="Logradouro" name="street" defaultValue={supplier?.street} slotProps={limit(FIELD_LIMITS.street)} />
      <TextField label="Número" name="number" defaultValue={supplier?.number} slotProps={limit(FIELD_LIMITS.number)} />
      <TextField label="Responsável" name="contactName" defaultValue={supplier?.contactName} slotProps={limit(FIELD_LIMITS.contactName)} />
      <TextField label="Telefone" name="phone" defaultValue={supplier?.phone} slotProps={limit(FIELD_LIMITS.phone)} />
      <TextField label="E-mail" name="email" type="email" defaultValue={supplier?.email} slotProps={limit(FIELD_LIMITS.email)} sx={{ gridColumn: { sm: "1 / -1" } }} />
    </Box>
  );
}
