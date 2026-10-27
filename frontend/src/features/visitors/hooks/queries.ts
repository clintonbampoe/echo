import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import {
  getVisitor,
  getVisitorSummary,
  listVisitors,
  searchVisitors,
  type VisitorFilters,
} from "../api/visitors";
import { visitorKeys } from "./keys";

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useVisitors = (filters: VisitorFilters) =>
  usePagedList(visitorKeys.list(filters), (c, s) =>
    listVisitors(filters, c, s),
  );

export const useVisitor = (id: string) =>
  useQuery({
    queryKey: visitorKeys.one(id),
    queryFn: ({ signal }) => getVisitor(id, signal),
  });

// Visitors whose name matches `q`. Waits for at least two letters; the caller debounces typing.
export const useVisitorSearch = (q: string) =>
  useQuery({
    queryKey: visitorKeys.search(q),
    queryFn: ({ signal }) => searchVisitors(q, signal),
    enabled: q.length >= 2,
  });

export const useVisitorSummary = (
  filters: Pick<VisitorFilters, "from" | "to">,
) =>
  useQuery({
    queryKey: visitorKeys.summary(filters),
    queryFn: ({ signal }) => getVisitorSummary(filters, signal),
  });
