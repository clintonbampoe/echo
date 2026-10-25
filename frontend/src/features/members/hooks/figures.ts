import { windowDates, type WindowKey } from "@/lib/windows";
import type { MemberFilters } from "../api/members";
import { memberFigures } from "../model/figures";
import { useMemberSummary } from "./queries";

// The numbers above the list. "New this year" is compared with last year, which is a second, cheap request that
// only goes out while the list is showing this year.
export function useMemberFigures(filters: MemberFilters, window: WindowKey) {
  const current = useMemberSummary(filters);
  const previous = useMemberSummary(
    { ...filters, ...windowDates("last-year") },
    window === "year",
  );
  return {
    stats: memberFigures(window, current.data, previous.data),
    busy: current.isPending,
  };
}
