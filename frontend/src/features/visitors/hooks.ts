// The only file in visitors that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRefreshMembers } from "@/features/members";
import { usePagedList } from "@/lib/paged";
import type { MemberInput } from "@/features/members";
import * as api from "./api";

export const visitorKeys = {
  all: ["visitors"] as const,
  list: (filters: api.VisitorFilters) => ["visitors", "list", filters] as const,
  summary: (filters: Pick<api.VisitorFilters, "from" | "to">) =>
    ["visitors", "summary", filters] as const,
  one: (id: string) => ["visitors", "one", id] as const,
};

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useVisitors = (filters: api.VisitorFilters) =>
  usePagedList(visitorKeys.list(filters), (c, s) =>
    api.listVisitors(filters, c, s),
  );

export const useVisitor = (id: string) =>
  useQuery({
    queryKey: visitorKeys.one(id),
    queryFn: ({ signal }) => api.getVisitor(id, signal),
  });

export const useVisitorSummary = (
  filters: Pick<api.VisitorFilters, "from" | "to">,
) =>
  useQuery({
    queryKey: visitorKeys.summary(filters),
    queryFn: ({ signal }) => api.getVisitorSummary(filters, signal),
  });

// Adding or changing a visitor changes the lists and the summary numbers, so refetch them all.
function useRefreshVisitors() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: visitorKeys.all });
}

export function useCreateVisitor() {
  const refresh = useRefreshVisitors();
  return useMutation({ mutationFn: api.createVisitor, onSuccess: refresh });
}

export function useUpdateVisitor(id: string) {
  const refresh = useRefreshVisitors();
  return useMutation({
    mutationFn: (input: Partial<api.VisitorInput>) =>
      api.updateVisitor(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteVisitor() {
  const refresh = useRefreshVisitors();
  return useMutation({ mutationFn: api.deleteVisitor, onSuccess: refresh });
}

// Making a visitor a member changes both people lists. Settled, not just success: a 409 means someone
// else already did it, and the page should then show it.
export function useConvertVisitor(id: string) {
  const refreshVisitors = useRefreshVisitors();
  const refreshMembers = useRefreshMembers();
  return useMutation({
    mutationFn: (input: MemberInput) => api.convertVisitor(id, input),
    onSettled: () => Promise.all([refreshVisitors(), refreshMembers()]),
  });
}
