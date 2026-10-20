import type { ReactNode } from "react";
import Box from "@mui/material/Box";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import EmptyState from "./EmptyState";

export type Column<T> = {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
  align?: "left" | "right" | "center";
  fit?: boolean;
  strong?: boolean;
  numeric?: boolean;
  wrap?: boolean;
  hideBelow?: "sm" | "md" | "lg";
};

type Props<T> = {
  columns: Column<T>[];
  rows: T[];
  rowKey: (row: T) => string | number;
  actions?: (row: T) => ReactNode;
  emptyTitle?: string;
  emptyHint?: ReactNode;
  emptyAction?: ReactNode;
};

const hide = (below?: "sm" | "md" | "lg") => (below ? { display: { xs: "none", [below]: "table-cell" } } : undefined);

export default function DataTable<T>({ columns, rows, rowKey, actions, emptyTitle = "Nenhum registro encontrado", emptyHint, emptyAction }: Props<T>) {
  if (rows.length === 0) return <EmptyState title={emptyTitle} hint={emptyHint} action={emptyAction} />;

  return (
    <Box sx={{ overflowX: "auto" }}>
      <Table>
        <TableHead>
          <TableRow>
            {columns.map((column) => (
              <TableCell
                key={column.key}
                align={column.align ?? (column.numeric ? "right" : "left")}
                sx={{ ...(column.fit ? { width: "1%" } : undefined), ...hide(column.hideBelow) }}
              >
                {column.header}
              </TableCell>
            ))}
            {actions && <TableCell align="right" sx={{ width: "1%" }} aria-label="Ações" />}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={rowKey(row)}>
              {columns.map((column) => (
                <TableCell
                  key={column.key}
                  align={column.align ?? (column.numeric ? "right" : "left")}
                  sx={{
                    whiteSpace: column.wrap ? "normal" : "nowrap",
                    fontWeight: column.strong ? 600 : undefined,
                    fontVariantNumeric: column.numeric ? "tabular-nums" : undefined,
                    ...(column.wrap ? { minWidth: 200, maxWidth: 420 } : undefined),
                    ...hide(column.hideBelow),
                  }}
                >
                  {column.render(row)}
                </TableCell>
              ))}
              {actions && (
                <TableCell align="right" sx={{ whiteSpace: "nowrap", py: 0.5 }}>
                  <Box sx={{ display: "inline-flex", alignItems: "center", gap: 0.25 }}>{actions(row)}</Box>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
}
