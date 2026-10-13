import Alert from "@mui/material/Alert";
import AlertTitle from "@mui/material/AlertTitle";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import type { AppError } from "@/lib/result";

type Props = {
  error: AppError;
  retryHref?: string;
  onRetry?: () => void;
};

export default function ErrorAlert({ error, retryHref, onRetry }: Props) {
  const retry =
    retryHref || onRetry ? (
      <Button color="inherit" size="small" href={retryHref} onClick={onRetry}>
        Tentar novamente
      </Button>
    ) : undefined;

  return (
    <Alert severity="error" action={retry}>
      <AlertTitle>{error.title}</AlertTitle>
      {error.messages.length === 1 ? (
        error.messages[0]
      ) : (
        <Box component="ul" sx={{ m: 0, pl: 2.5 }}>
          {error.messages.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </Box>
      )}
    </Alert>
  );
}
