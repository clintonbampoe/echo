// The only file in tithes that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const titheKeys = {
  all: ["tithes"] as const,
  list: (filters: api.TitheFilters) => ["tithes", "list", filters] as const,
  summary: (filters: api.TitheFilters) =>
    ["tithes", "summary", filters] as const,
  member: (id: string, filters: api.TitheFilters) =>
    ["tithes", "member", id, filters] as const,
  one: (id: string) => ["tithes", "one", id] as const,
  sameMonth: (memberId: string, year: number, month: string) =>
    ["tithes", "same-month", memberId, year, month] as const,
  months: (who: api.TitheMonthsFor, from?: string, to?: string) =>
    ["tithes", "months", who, from, to] as const,
};

// Every tithe the church recorded that matches the filters, newest first.
export const useTithes = (filters: api.TitheFilters) =>
  usePagedList(titheKeys.list(filters), (c, s) =>
    api.listTithes(filters, c, s),
  );

// One tithe by its id (a link to it from another page).
export const useTithe = (id: string) =>
  useQuery({
    queryKey: titheKeys.one(id),
    queryFn: ({ signal }) => api.getTithe(id, signal),
  });

export const useTitheSummary = (filters: api.TitheFilters) =>
  useQuery({
    queryKey: titheKeys.summary(filters),
    queryFn: ({ signal }) => api.getTitheSummary(filters, signal),
  });

// The tithes one member gave that match the filters (all of them with none), newest first.
export const useMemberTithes = (
  memberId: string,
  filters: api.TitheFilters = {},
) =>
  usePagedList(titheKeys.member(memberId, filters), (c, s) =>
    api.listMemberTithes(memberId, filters, c, s),
  );

// What a member already has for one month (the tithes a new one might double). Waits until a member is chosen.
export const useSameMonthTithes = (
  memberId: string | undefined,
  year: number,
  month: string,
) =>
  useQuery({
    queryKey: titheKeys.sameMonth(memberId ?? "", year, month),
    queryFn: ({ signal }) =>
      api.listTithes({ memberId, year, month }, undefined, signal),
    enabled: Boolean(memberId),
  });

// Month by month totals for one member or for the church (with no range: the last twelve months).
export const useTitheMonths = (
  who: api.TitheMonthsFor,
  from?: string,
  to?: string,
) =>
  useQuery({
    queryKey: titheKeys.months(who, from, to),
    queryFn: ({ signal }) => api.getTitheMonths(who, { from, to }, signal),
  });

// Recording, changing or deleting a tithe changes every list, number and chart, and a member's profile.
function useRefreshTithes() {
  const client = useQueryClient();
  // Not awaited: the form that saved moves on at once, and the lists refresh behind it.
  return () => void client.invalidateQueries({ queryKey: titheKeys.all });
}

export function useCreateTithe() {
  const refresh = useRefreshTithes();
  return useMutation({ mutationFn: api.createTithe, onSuccess: refresh });
}

export function useUpdateTithe() {
  const refresh = useRefreshTithes();
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & Partial<api.TitheUpdate>) =>
      api.updateTithe(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteTithe() {
  const refresh = useRefreshTithes();
  return useMutation({ mutationFn: api.deleteTithe, onSuccess: refresh });
}
