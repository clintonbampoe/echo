import { get } from "./client";

export const getTithes = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalCollected: number }>("/v1/tithes/summary", from, to, signal).then(
    (r) => r.totalCollected,
  );

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
