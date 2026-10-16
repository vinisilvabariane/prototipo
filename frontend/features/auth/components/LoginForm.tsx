"use client";

import { startTransition, useActionState, type FormEvent } from "react";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { keyframes } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import ErrorAlert from "@/components/ui/ErrorAlert";
import LogoMark from "@/components/ui/LogoMark";
import { easeOut } from "@/lib/tokens";
import { login } from "../actions/authActions";

const rise = keyframes`
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: none; }
`;

const enter = (index: number) => ({
  "@media (prefers-reduced-motion: no-preference)": { animation: `${rise} 0.7s ${easeOut} ${index * 80}ms backwards` },
});

export default function LoginForm() {
  const [result, formAction, pending] = useActionState(login, undefined);

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    startTransition(() => formAction(formData));
  }

  return (
    <Stack spacing={4} sx={{ width: "100%", maxWidth: 380 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" spacing={1.25} sx={{ alignItems: "center", display: { md: "none" }, ...enter(0) }}>
          <LogoMark />
          <Typography sx={{ fontWeight: 600, fontSize: "1.2rem", letterSpacing: "-0.03em" }}>
            MeOne
          </Typography>
        </Stack>
        <Typography variant="h4" component="h1" sx={enter(1)}>
          Entrar
        </Typography>
        <Typography color="text.secondary" sx={enter(2)}>
          Use sua matrícula e senha para acessar.
        </Typography>
      </Stack>

      <Stack component="form" onSubmit={handleSubmit} spacing={2} sx={enter(3)}>
        {result && !result.ok && <ErrorAlert error={result.error} />}
        <TextField
          label="Matrícula"
          name="registration"
          autoComplete="username"
          slotProps={{ htmlInput: { inputMode: "numeric" } }}
          required
          fullWidth
        />
        <TextField label="Senha" name="password" type="password" autoComplete="current-password" required fullWidth />
        <Button
          type="submit"
          variant="contained"
          size="large"
          disabled={pending}
          sx={{ py: 1.4 }}
        >
          {pending ? "Verificando..." : "Entrar"}
        </Button>
      </Stack>
    </Stack>
  );
}
