import { get, getMonthly } from "./client";

// Tithes collected in the range, and in the stretch of the same length before it: one request for both.
export type Collected = { total: number; before: number };

export const getTithes = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalCollected: number; previousTotalCollected: number }>(
    "/v1/tithes/summary",
    from,
    to,
    signal,
    { includePrevious: true },
  ).then((r): Collected => ({
    total: r.totalCollected,
    before: r.previousTotalCollected,
  }));

// What the Tithes card shows for one stretch.
export type TitheFigures = { total: number; tithers: number; average: number };

export const getTitheFigures = (
  from: string,
  to: string,
  signal?: AbortSignal,
) =>
  get<{
    totalCollected: number;
    uniqueTithers: number;
    averagePerMember: number;
  }>("/v1/tithes/summary", from, to, signal).then((r): TitheFigures => ({
    total: r.totalCollected,
    tithers: r.uniqueTithers,
    average: r.averagePerMember,
  }));

// The same figures for each of `months` months ending with the month of `to`. Tithes count in the month they are for.
export type TitheMonth = TitheFigures & { year: number; month: number };

export const getTitheMonths = (
  months: number,
  to: string,
  signal?: AbortSignal,
) =>
  getMonthly<{
    year: number;
    month: number;
    totalCollected: number;
    uniqueTithers: number;
    averagePerMember: number;
  }>("/v1/tithes/summary/monthly", months, to, signal).then((rows) =>
    rows.map((r): TitheMonth => ({
      year: r.year,
      month: r.month,
      total: r.totalCollected,
      tithers: r.uniqueTithers,
      average: r.averagePerMember,
    })),
  );
