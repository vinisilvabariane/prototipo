import Stack from "@mui/material/Stack";
import type { AppError } from "@/lib/result";
import ErrorAlert from "./ErrorAlert";
import PageHeader from "./PageHeader";

export default function PageError({ title, error, retryHref }: { title: string; error: AppError; retryHref: string }) {
  return (
    <Stack spacing={2.5}>
      <PageHeader title={title} />
      <ErrorAlert error={error} retryHref={retryHref} />
    </Stack>
  );
}
