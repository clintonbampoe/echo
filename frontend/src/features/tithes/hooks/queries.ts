import { useQuery } from "@tanstack/react-query";
import type { PaymentMethod } from "@/features/transactions";
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

// What each way of paying brought in, for the "How it was paid" card. The summary already carries the split, so
// with no method chosen this shares the page's own summary request. A chosen method narrows the summary to itself,
// so the split asks without it (one more request) and every way of paying stays in view.
export const useTitheMethods = (filters: TitheFilters) => {
  const { paymentMethod: _chosen, ...wider } = filters;
  return useQuery({
    queryKey: titheKeys.summary(wider),
    queryFn: ({ signal }) => getTitheSummary(wider, signal),
    select: (s) =>
      (s.byMethod ?? []).map((m) => ({
        method: m.method as PaymentMethod,
        total: m.total,
      })),
  });
};

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

// Month by month totals for one member or for the church: `months` months ending with the month of `to`.
export const useTitheMonths = (
  who: TitheMonthsFor,
  months: number,
  to: string,
) =>
  useQuery({
    queryKey: titheKeys.months(who, months, to),
    queryFn: ({ signal }) => getTitheMonths(who, months, to, signal),
  });
