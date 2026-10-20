"use client";

import { startTransition, useActionState, type FormEvent } from "react";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import ErrorAlert from "@/components/ui/ErrorAlert";
import Panel from "@/components/ui/Panel";
import type { Result } from "@/lib/result";
import { saveSupplier } from "../actions/supplierActions";
import type { SupplierDetail } from "../types";
import SupplierFields from "./SupplierFields";

export default function SupplierForm({ supplier }: { supplier?: SupplierDetail }) {
  const [result, formAction, pending] = useActionState(
    (prev: Result | undefined, formData: FormData) => saveSupplier(supplier?.id ?? null, prev, formData),
    undefined,
  );

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    startTransition(() => formAction(formData));
  }

  return (
    <Box component="form" onSubmit={handleSubmit}>
      <Stack spacing={2.5}>
        {result && !result.ok && <ErrorAlert error={result.error} />}
        <Panel title="Dados do fornecedor">
          <SupplierFields supplier={supplier} />
        </Panel>
        <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
          <Button href={supplier ? `/suppliers/${supplier.id}` : "/suppliers"}>Cancelar</Button>
          <Button type="submit" variant="contained" disabled={pending}>
            {pending ? "Salvando..." : supplier ? "Salvar alterações" : "Cadastrar fornecedor"}
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}
