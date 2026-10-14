// The only file in tithes that knows about TanStack Query.
import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const titheKeys = {
  member: (id: string) => ["tithes", "member", id] as const,
  total: (id: string) => ["tithes", "total", id] as const,
  months: (id: string, from: string, to: string) =>
    ["tithes", "months", id, from, to] as const,
};

// Every tithe one member gave, newest first.
export const useMemberTithes = (memberId: string) =>
  usePagedList(titheKeys.member(memberId), (c, s) =>
    api.listMemberTithes(memberId, c, s),
  );

export const useTitheTotalEver = (memberId: string, enabled: boolean) =>
  useQuery({
    queryKey: titheKeys.total(memberId),
    queryFn: ({ signal }) => api.getTitheTotalEver(memberId, signal),
    enabled,
  });

export const useTitheMonths = (memberId: string, from: string, to: string) =>
  useQuery({
    queryKey: titheKeys.months(memberId, from, to),
    queryFn: ({ signal }) => api.getTitheMonths(memberId, from, to, signal),
  });
