import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import type { TitheFilters, TitheMonthsFor } from "../api/tithes";
import {
  getTithe,
  getTitheMonths,
  getTitheSummary,
  listMemberTithes,
  listTithes,
} from "../api/tithes";
import { titheKeys } from "./keys";

// Every tithe the church recorded that matches the filters, newest first.
export const useTithes = (filters: TitheFilters) =>
  usePagedList(titheKeys.list(filters), (c, s) => listTithes(filters, c, s));

// One tithe by its id (a link to it from another page).
export const useTithe = (id: string) =>
  useQuery({
    queryKey: titheKeys.one(id),
    queryFn: ({ signal }) => getTithe(id, signal),
  });

export const useTitheSummary = (filters: TitheFilters) =>
  useQuery({
    queryKey: titheKeys.summary(filters),
    queryFn: ({ signal }) => getTitheSummary(filters, signal),
  });

// The tithes one member gave that match the filters (all of them with none), newest first.
export const useMemberTithes = (memberId: string, filters: TitheFilters = {}) =>
  usePagedList(titheKeys.member(memberId, filters), (c, s) =>
    listMemberTithes(memberId, filters, c, s),
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
      listTithes({ memberId, year, month }, undefined, signal),
    enabled: Boolean(memberId),
  });

// Month by month totals for one member or for the church (with no range: the last twelve months).
export const useTitheMonths = (
  who: TitheMonthsFor,
  from?: string,
  to?: string,
) =>
  useQuery({
    queryKey: titheKeys.months(who, from, to),
    queryFn: ({ signal }) => getTitheMonths(who, { from, to }, signal),
  });
