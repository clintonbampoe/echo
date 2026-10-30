import { api } from "@/lib/api";

export const get = <T>(
  path: string,
  from: string,
  to: string,
  signal?: AbortSignal,
  extra?: Record<string, string | number | boolean>,
) => api.get<T>(path, { query: { from, to, ...extra }, signal });

// A `/summary/monthly` endpoint: one row per calendar month, oldest first, for `months` months (1 to 12) ending
// with the month of `to`. Months with nothing in them come back as zeros.
export const getMonthly = <T>(
  path: string,
  months: number,
  to: string,
  signal?: AbortSignal,
) => api.get<T[]>(path, { query: { months, to }, signal });
