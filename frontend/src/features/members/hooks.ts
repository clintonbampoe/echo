// The only file in members that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const memberKeys = {
  all: ["members"] as const,
  list: (filters: api.MemberFilters) => ["members", "list", filters] as const,
  summary: (filters: api.MemberFilters) =>
    ["members", "summary", filters] as const,
  one: (id: string) => ["members", "one", id] as const,
};

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useMembers = (filters: api.MemberFilters) =>
  usePagedList(memberKeys.list(filters), (c, s) =>
    api.listMembers(filters, c, s),
  );

export const useMember = (id: string) =>
  useQuery({
    queryKey: memberKeys.one(id),
    queryFn: ({ signal }) => api.getMember(id, signal),
  });

export const useMemberSummary = (filters: api.MemberFilters) =>
  useQuery({
    queryKey: memberKeys.summary(filters),
    queryFn: ({ signal }) => api.getMemberSummary(filters, signal),
  });

// Adding or changing a member changes the lists and the summary numbers, so refetch them all.
function useRefreshMembers() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: memberKeys.all });
}

export function useCreateMember() {
  const refresh = useRefreshMembers();
  return useMutation({ mutationFn: api.createMember, onSuccess: refresh });
}

export function useDeleteMember() {
  const refresh = useRefreshMembers();
  return useMutation({ mutationFn: api.deleteMember, onSuccess: refresh });
}

export function useUpdateMember(id: string) {
  const refresh = useRefreshMembers();
  return useMutation({
    mutationFn: (input: Partial<api.MemberInput>) =>
      api.updateMember(id, input),
    onSuccess: refresh,
  });
}
