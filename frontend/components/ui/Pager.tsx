import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

type Props = {
  basePath: string;
  page: number;
  pageSize: number;
  totalCount: number;
  params?: Record<string, string | undefined>;
};

export default function Pager({ basePath, page, pageSize, totalCount, params = {} }: Props) {
  const lastPage = Math.max(0, Math.ceil(totalCount / pageSize) - 1);

  function href(target: number) {
    const query = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) if (value) query.set(key, value);
    query.set("page", String(target));
    return `${basePath}?${query}`;
  }

  return (
    <Stack direction="row" spacing={2} sx={{ p: 2, alignItems: "center", justifyContent: "flex-end" }}>
      <Typography variant="body2" color="text.secondary">
        Página {page + 1} de {lastPage + 1} · {totalCount} registros
      </Typography>
      <Button size="small" href={href(page - 1)} disabled={page <= 0}>
        Anterior
      </Button>
      <Button size="small" href={href(page + 1)} disabled={page >= lastPage}>
        Próxima
      </Button>
    </Stack>
  );
}
