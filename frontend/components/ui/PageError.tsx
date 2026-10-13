import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { AppError } from "@/lib/result";
import ErrorAlert from "./ErrorAlert";

export default function PageError({ title, error, retryHref }: { title: string; error: AppError; retryHref: string }) {
  return (
    <Stack spacing={3}>
      <Typography variant="h4">{title}</Typography>
      <ErrorAlert error={error} retryHref={retryHref} />
    </Stack>
  );
}
