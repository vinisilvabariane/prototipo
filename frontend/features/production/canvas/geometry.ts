export const GRID = 30;
export const POST_WIDTH = 210;
export const POST_HEIGHT = 120;
export const MIN_POST_WIDTH = 150;
export const MIN_POST_HEIGHT = 90;
export const MIN_DECORATION_SIZE = 30;

export const snap = (value: number) => Math.round(value / GRID) * GRID;

export type Box = { x: number; y: number; width: number; height: number };

const overlaps = (a: Box, b: Box) =>
  a.x < b.x + b.width + GRID && b.x < a.x + a.width + GRID && a.y < b.y + b.height + GRID && b.y < a.y + a.height + GRID;

export function findFreeSpot(existing: Box[], width: number, height: number, columns = 1800): { x: number; y: number } {
  for (let y = GRID; y < 60_000; y += GRID) {
    for (let x = GRID; x + width < columns; x += GRID) {
      const candidate = { x, y, width, height };
      if (!existing.some((box) => overlaps(candidate, box))) return { x, y };
    }
  }
  return { x: GRID, y: GRID };
}

export function arrangeInGrid(boxes: Box[], columns = 1800): { x: number; y: number }[] {
  if (boxes.length === 0) return [];

  const cellWidth = snap(Math.max(...boxes.map((box) => box.width)) + GRID);
  const cellHeight = snap(Math.max(...boxes.map((box) => box.height)) + GRID);
  const perRow = Math.max(1, Math.floor((columns - GRID) / cellWidth));

  return boxes.map((_, index) => ({
    x: GRID + (index % perRow) * cellWidth,
    y: GRID + Math.floor(index / perRow) * cellHeight,
  }));
}
