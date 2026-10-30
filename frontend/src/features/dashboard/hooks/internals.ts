import { useQuery } from "@tanstack/react-query";
import type { Bucket } from "../model/trend";

// One answer for each month a chart draws, from a single `/summary/monthly` call. The server sends a row for every
// month, so `pick` turns the row of a bar's month (or nothing, were it missing) into what the bar needs.
export function useMonthly<Row extends { year: number; month: number }, T>(
  name: string,
  buckets: readonly Bucket[],
  fetchRows: (
    months: number,
    to: string,
    signal?: AbortSignal,
  ) => Promise<Row[]>,
  pick: (row: Row | undefined) => T,
) {
  const months = buckets.length;
  const to = buckets[months - 1]?.to ?? "";
  const query = useQuery({
    queryKey: ["dashboard", name, months, to],
    queryFn: ({ signal }) => fetchRows(months, to, signal),
    enabled: months > 0,
  });
  const values = query.data
    ? buckets.map((b) =>
        pick(
          query.data.find(
            (r) =>
              `${r.year}-${String(r.month).padStart(2, "0")}` ===
              b.from.slice(0, 7),
          ),
        ),
      )
    : undefined;
  return {
    values,
    failed: query.isError,
    retry: () => void query.refetch(),
  };
}

// One answer from a summary call, with the same three things every card needs: it, whether it failed, a retry.
export function useData<T>(
  name: string,
  from: string,
  to: string,
  fetchData: (from: string, to: string, signal?: AbortSignal) => Promise<T>,
) {
  const query = useQuery({
    queryKey: ["dashboard", name, from, to],
    queryFn: ({ signal }) => fetchData(from, to, signal),
  });
  return {
    data: query.data,
    failed: query.isError,
    retry: () => void query.refetch(),
  };
}

// One answer that has no range: the same whatever the period.
export function useOnce<T>(
  name: string,
  fetchData: (signal?: AbortSignal) => Promise<T>,
) {
  const query = useQuery({
    queryKey: ["dashboard", name],
    queryFn: ({ signal }) => fetchData(signal),
  });
  return {
    data: query.data,
    failed: query.isError,
    retry: () => void query.refetch(),
  };
}
