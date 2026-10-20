import ChevronLeft from "@mui/icons-material/ChevronLeft";
import ChevronRight from "@mui/icons-material/ChevronRight";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { inkSoft, line } from "@/lib/tokens";

type Props = {
  basePath: string;
  page: number;
  pageSize: number;
  totalCount: number;
  params?: Record<string, string | undefined>;
};

export default function Pager({ basePath, page, pageSize, totalCount, params = {} }: Props) {
  if (totalCount === 0) return null;

  const lastPage = Math.max(0, Math.ceil(totalCount / pageSize) - 1);
  const from = page * pageSize + 1;
  const to = Math.min(totalCount, (page + 1) * pageSize);

  function href(target: number) {
    const query = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) if (value) query.set(key, value);
    query.set("page", String(target));
    return `${basePath}?${query}`;
  }

  return (
    <Stack direction="row" spacing={1} sx={{ px: 2, py: 0.75, alignItems: "center", justifyContent: "space-between", borderTop: `1px solid ${line}` }}>
      <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
        {from}–{to} de {totalCount}
      </Typography>
      {lastPage > 0 && (
        <Stack direction="row" spacing={0.5} sx={{ alignItems: "center" }}>
          <Typography variant="body2" sx={{ color: inkSoft, mr: 0.5, fontVariantNumeric: "tabular-nums" }}>
            Página {page + 1} de {lastPage + 1}
          </Typography>
          <IconButton size="small" href={href(page - 1)} disabled={page <= 0} aria-label="Página anterior">
            <ChevronLeft fontSize="small" />
          </IconButton>
          <IconButton size="small" href={href(page + 1)} disabled={page >= lastPage} aria-label="Próxima página">
            <ChevronRight fontSize="small" />
          </IconButton>
        </Stack>
      )}
    </Stack>
  );
}
