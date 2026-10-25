import { useQueries, useQuery } from "@tanstack/react-query";
import type { Amount } from "../model/cards";
import type { Bucket } from "../model/trend";

// One number from a summary call. The key holds the range, so asking for the same one twice shares it.
export function useAmount(
  name: string,
  from: string,
  to: string,
  fetchAmount: (
    from: string,
    to: string,
    signal?: AbortSignal,
  ) => Promise<number>,
): Amount {
  const query = useQuery({
    queryKey: ["dashboard", name, from, to],
    queryFn: ({ signal }) => fetchAmount(from, to, signal),
  });
  return {
    value: query.data,
    failed: query.isError,
    retry: () => void query.refetch(),
  };
}

// One answer for each stretch, in order. `values` is there only once every stretch has answered.
export function useBuckets<T>(
  name: string,
  buckets: readonly Bucket[],
  fetchOne: (from: string, to: string, signal?: AbortSignal) => Promise<T>,
) {
  const results = useQueries({
    queries: buckets.map((b) => ({
      queryKey: ["dashboard", name, b.from, b.to],
      queryFn: ({ signal }: { signal?: AbortSignal }) =>
        fetchOne(b.from, b.to, signal),
    })),
  });
  const values = results.flatMap((r) => (r.data === undefined ? [] : [r.data]));
  return {
    values: values.length === results.length ? values : undefined,
    failed: results.some((r) => r.isError),
    retry: () =>
      results.forEach((r) => {
        if (r.isError) void r.refetch();
      }),
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
