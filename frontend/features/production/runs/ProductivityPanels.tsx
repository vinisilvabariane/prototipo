import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { brand, inkSoft, line } from "@/lib/tokens";
import { formatHour, formatMinutes, formatPercent, formatQuantity } from "../format";
import { POST_KIND_LABELS } from "../labels";
import type { RunDetail } from "../types";

function Panel({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Card sx={{ overflow: "hidden" }}>
      <Typography variant="overline" component="h2" sx={{ display: "block", color: inkSoft, px: 2.5, pt: 2, pb: 0.5 }}>
        {title}
      </Typography>
      {children}
    </Card>
  );
}

function Empty({ children }: { children: ReactNode }) {
  return <Typography sx={{ color: inkSoft, px: 2.5, pb: 2.5, pt: 1 }}>{children}</Typography>;
}

function attainmentColor(value: number | null) {
  if (value === null) return line;
  if (value >= 1) return "var(--mui-palette-success-main)";
  if (value >= 0.8) return brand;
  return "var(--mui-palette-warning-main)";
}

export default function ProductivityPanels({ run }: { run: RunDetail }) {
  const started = run.status !== "preparing";
  const maxHour = Math.max(1, ...run.hours.map((hour) => hour.produced));

  return (
    <Box sx={{ display: "grid", gap: 3 }}>
      <Panel title="Produtividade por posto">
        {!started ? (
          <Empty>Os números aparecem quando a produção começar.</Empty>
        ) : (
          <Box sx={{ overflowX: "auto" }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Posto</TableCell>
                  <TableCell>Quem está</TableCell>
                  <TableCell align="right">Meta por hora</TableCell>
                  <TableCell align="right">Tempo ocupado</TableCell>
                  <TableCell align="right">Esperado</TableCell>
                  <TableCell align="right">Produzido</TableCell>
                  <TableCell sx={{ minWidth: 150 }}>Atingimento</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {run.posts.map((post) => (
                  <TableRow key={post.id}>
                    <TableCell>
                      <Typography sx={{ fontWeight: 600 }} component="span">
                        {post.name}
                      </Typography>
                      <Typography component="span" variant="body2" sx={{ color: inkSoft, ml: 1 }}>
                        {POST_KIND_LABELS[post.kind]}
                      </Typography>
                    </TableCell>
                    <TableCell>{post.allocation?.userName ?? "—"}</TableCell>
                    <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                      {formatQuantity(post.targetPerHour)}
                    </TableCell>
                    <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                      {formatMinutes(post.occupiedMinutes)}
                    </TableCell>
                    <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                      {formatQuantity(post.expected)}
                    </TableCell>
                    <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums", fontWeight: 600 }}>
                      {formatQuantity(post.produced)}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                        <Box sx={{ flex: 1, height: 6, borderRadius: 3, bgcolor: line, overflow: "hidden" }}>
                          <Box sx={{ height: "100%", width: `${Math.min(100, Math.round((post.attainment ?? 0) * 100))}%`, bgcolor: attainmentColor(post.attainment) }} />
                        </Box>
                        <Typography variant="body2" sx={{ width: 40, textAlign: "right", fontWeight: 600, fontVariantNumeric: "tabular-nums" }}>
                          {formatPercent(post.attainment)}
                        </Typography>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>
        )}
      </Panel>

      <Box sx={{ display: "grid", gap: 3, gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 1fr) minmax(0, 1fr)" }, alignItems: "start" }}>
        <Panel title="Pessoas">
          {run.people.length === 0 ? (
            <Empty>Ninguém foi alocado ainda.</Empty>
          ) : (
            <Box sx={{ overflowX: "auto" }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Pessoa</TableCell>
                    <TableCell align="right">Tempo no posto</TableCell>
                    <TableCell align="right">Postos agora</TableCell>
                    <TableCell align="right">Produzido</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {run.people.map((person) => (
                    <TableRow key={person.userId}>
                      <TableCell>
                        <Typography sx={{ fontWeight: 600 }} component="span">
                          {person.userName}
                        </Typography>
                        <Typography component="span" variant="body2" sx={{ color: inkSoft, ml: 1, fontVariantNumeric: "tabular-nums" }}>
                          {person.registration}
                        </Typography>
                      </TableCell>
                      <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                        {formatMinutes(person.minutes)}
                      </TableCell>
                      <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                        {person.openPosts}
                      </TableCell>
                      <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums", fontWeight: 600 }}>
                        {formatQuantity(person.produced)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          )}
        </Panel>

        <Panel title="Produção por hora">
          {run.hours.length === 0 ? (
            <Empty>{started ? "Nenhuma produção lançada ainda." : "Os lançamentos aparecem quando a produção começar."}</Empty>
          ) : (
            <Box sx={{ display: "grid", gap: 1, px: 2.5, pb: 2.5, pt: 1 }}>
              {run.hours.map((hour) => (
                <Box key={hour.hourStart} sx={{ display: "grid", gridTemplateColumns: "44px minmax(0, 1fr) 56px", alignItems: "center", gap: 1.5 }}>
                  <Typography variant="body2" sx={{ color: inkSoft, fontVariantNumeric: "tabular-nums" }}>
                    {formatHour(hour.hourStart)}
                  </Typography>
                  <Box sx={{ height: 10, borderRadius: 2, bgcolor: line, overflow: "hidden" }}>
                    <Box sx={{ height: "100%", width: `${Math.round((hour.produced / maxHour) * 100)}%`, bgcolor: brand }} />
                  </Box>
                  <Typography variant="body2" sx={{ textAlign: "right", fontWeight: 600, fontVariantNumeric: "tabular-nums" }}>
                    {formatQuantity(hour.produced)}
                  </Typography>
                </Box>
              ))}
            </Box>
          )}
        </Panel>
      </Box>
    </Box>
  );
}
