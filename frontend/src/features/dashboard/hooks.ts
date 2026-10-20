// The only file in the dashboard that knows about TanStack Query.
import { useQueries, useQuery } from "@tanstack/react-query";
import { ALL_TIME } from "@/lib/dates";
import * as api from "./api";
import type { Amount, Numbers } from "./cards";
import { previousSpan, type Span } from "./periods";
import type { Bucket } from "./trend";

// One number from a summary call. The key holds the range, so asking for the same one twice shares it.
function useAmount(
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

// Members who joined in a range. The server counts a member with no joined date in every range, so this
// asks for a single day long ago (which only those members match) and takes that many off.
function useUndatedMembers() {
  return useOnce("members-undated", (signal) =>
    api.getMembers(ALL_TIME.from, ALL_TIME.from, signal),
  );
}

// People who joined in a range, without the members who have no joined date.
export function useNewMembers(span: Span): Amount {
  const joined = useAmount("members", span.from, span.to, api.getMembers);
  const undated = useUndatedMembers();
  const gone = undated.data;
  return {
    value:
      joined.value !== undefined && gone !== undefined
        ? Math.max(0, joined.value - gone)
        : undefined,
    failed: joined.failed || undated.failed,
    retry: () => {
      joined.retry();
      undated.retry();
    },
  };
}

// Everything the four cards need: the period, and the stretch of the same length before it.
// Members is the whole church as of today, so it does not change with the period.
export function useNumbers(span: Span, today: string): Numbers {
  const before = previousSpan(span);
  return {
    members: useAmount("members", ALL_TIME.from, today, api.getMembers),
    newMembers: useNewMembers(span),
    present: useAmount("present", span.from, span.to, api.getPresent),
    presentBefore: useAmount("present", before.from, before.to, api.getPresent),
    tithes: useAmount("tithes", span.from, span.to, api.getTithes),
    tithesBefore: useAmount("tithes", before.from, before.to, api.getTithes),
    net: useAmount("net", span.from, span.to, api.getNet),
    netBefore: useAmount("net", before.from, before.to, api.getNet),
  };
}

// One answer for each stretch, in order. `values` is there only once every stretch has answered.
function useBuckets<T>(
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

export type Series = {
  values: number[] | undefined;
  failed: boolean;
  retry: () => void;
};

// People present in each stretch. The keys match the Present card's, so last week is asked for once.
export const useSeries = (buckets: readonly Bucket[]): Series =>
  useBuckets("present", buckets, api.getPresent);

// Tithes, tithers and the average tithe in each stretch.
export const useTitheSeries = (buckets: readonly Bucket[]) =>
  useBuckets("tithe-figures", buckets, api.getTitheFigures);

// New members in each stretch, without the members who have no joined date.
export function useGrowth(buckets: readonly Bucket[]): Series {
  const series = useBuckets("members", buckets, api.getMembers);
  const undated = useUndatedMembers();
  const gone = undated.data;
  return {
    values:
      series.values && gone !== undefined
        ? series.values.map((v) => Math.max(0, v - gone))
        : undefined,
    failed: series.failed || undated.failed,
    retry: () => {
      series.retry();
      undated.retry();
    },
  };
}

// One answer from a summary call, with the same three things every card needs: it, whether it failed, a retry.
function useData<T>(
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

// The money of each month in the range.
export function useMoneyMonths(from: string, to: string) {
  const { data, failed, retry } = useData("months", from, to, api.getMonths);
  return { months: data, failed, retry };
}

// What each category added up to in the range.
export function useCategories(from: string, to: string) {
  const { data, failed, retry } = useData(
    "categories",
    from,
    to,
    api.getCategories,
  );
  return { categories: data, failed, retry };
}

const NEXT_EVENTS = 5;

// The next few events that have not started yet, soonest first.
export function useUpcomingEvents(today: string) {
  const { data, failed, retry } = useData(
    "events-next",
    today,
    ALL_TIME.to,
    (from, to, signal) => api.getEvents(from, to, "Asc", NEXT_EVENTS, signal),
  );
  return { events: data, failed, retry };
}

// Every event that starts in a range, up to the server's page of 50.
export function useEventsBetween(from: string, to: string) {
  const { data, failed, retry } = useData(
    "events-in",
    from,
    to,
    (f, t, signal) => api.getEvents(f, t, "Asc", 50, signal),
  );
  return { events: data, failed, retry };
}

// Visitors added in the range, and how many became members.
export function useVisitors(from: string, to: string) {
  const { data, failed, retry } = useData(
    "visitors",
    from,
    to,
    api.getVisitorFigures,
  );
  return { figures: data, failed, retry };
}

// One answer that has no range: the same whatever the period.
function useOnce<T>(
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

// Every project and what it has raised, all time.
export function useProjects() {
  const { data, failed, retry } = useOnce("projects", api.getProjects);
  return { projects: data, failed, retry };
}

// How many members are men and women, and their age groups. One answer, shared by two cards.
export function useMix() {
  const { data, failed, retry } = useOnce("mix", api.getMix);
  return { mix: data, failed, retry };
}

// The newest visitors, over the widest window the card admits. The server sends a page in name order,
// so the sorting into newest first happens on the card, from the days the server said. No period.
export function useRecentVisitors(today: string) {
  const { data, failed, retry } = useData("visitors-recent", today, today, (_f, t, signal) =>
    api.getRecentVisitors(t, signal),
  );
  return { visitors: data, failed, retry };
}

// The projects At Risk (the server's own status) and, of every project, how many are At Risk. No period.
export function useProjectsAtRisk() {
  const rows = useOnce("projects-at-risk", api.getProjectsAtRisk);
  const totals = useOnce("project-totals", api.getProjectTotals);
  return {
    projects: rows.data,
    totals: totals.data,
    failed: rows.failed || totals.failed,
    retry: () => {
      rows.retry();
      totals.retry();
    },
  };
}

// The assets as a whole, and each category's share. The server has no breakdown by category, so this asks
// once for the totals, once for the categories and once for each category that holds assets. `rows` is there
// only when every one has answered.
export function useAssets() {
  const overall = useOnce("asset-figures", (signal) =>
    api.getAssetFigures(undefined, signal),
  );
  const categories = useOnce("asset-categories", api.getAssetCategories);
  const used = (categories.data ?? []).filter((c) => c.assetCount > 0);
  const each = useQueries({
    queries: used.map((c) => ({
      queryKey: ["dashboard", "asset-figures", c.id],
      queryFn: ({ signal }: { signal?: AbortSignal }) =>
        api.getAssetFigures(c.id, signal),
    })),
  });
  const rows = each.flatMap((r, i) => {
    const c = used[i];
    return r.data && c
      ? [
          {
            key: String(c.id),
            name: c.name,
            count: r.data.count,
            value: r.data.value,
          },
        ]
      : [];
  });
  const ready =
    overall.data !== undefined &&
    categories.data !== undefined &&
    rows.length === used.length;
  return {
    figures: overall.data,
    rows: ready ? rows : undefined,
    failed: overall.failed || categories.failed || each.some((r) => r.isError),
    retry: () => {
      overall.retry();
      categories.retry();
      each.forEach((r) => {
        if (r.isError) void r.refetch();
      });
    },
  };
}
