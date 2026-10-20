// Cuts a list of amounts into the slices of a ring: the biggest four, and the rest together as "Other". Pure.
export type Row = { key: string; name: string; total: number };
export type Slice = Row & { percent: number; color: string };

// Darkest for the biggest, lightest for "Other".
const RAMP = ["#1F2A3A", "#4F6580", "#7F95AE", "#B3C2D3", "#DDE4EC"];
const LIGHTEST = "#DDE4EC";
const NAMED = 4;

const sum = (rows: readonly Row[]) => rows.reduce((n, r) => n + r.total, 0);

export function shareOf(rows: readonly Row[]): {
  total: number;
  slices: Slice[];
} {
  const sorted = rows
    .filter((r) => r.total > 0)
    .sort((a, b) => b.total - a.total);
  const total = sum(sorted);
  const kept =
    sorted.length > NAMED + 1
      ? [
          ...sorted.slice(0, NAMED),
          { key: "other", name: "Other", total: sum(sorted.slice(NAMED)) },
        ]
      : sorted;
  return {
    total,
    slices: kept.map((r, i) => ({
      ...r,
      percent: Math.round((r.total / total) * 100),
      color: RAMP[i] ?? LIGHTEST,
    })),
  };
}
