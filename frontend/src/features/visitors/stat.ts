// The four numbers at the top of a visitor's page. Pure, so the tests need no screen.
import { daysBetween } from "@/lib/dates";
import { formatDate, formatDayMonth, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { Visitor } from "./api";

export const visitsStat = (v: Pick<Visitor, "visitCount">): Stat => ({
  label: "Visits",
  value: formatNumber(v.visitCount),
  note: "As a visitor",
});

export const firstVisitStat = (v: Pick<Visitor, "createdAt">): Stat => ({
  label: "First visit",
  value: formatDayMonth(v.createdAt),
  note: "First recorded",
});

// How long since they last came: the number to act on, so the note says so.
export function daysSinceStat(
  v: Pick<Visitor, "lastVisitedAt">,
  today: string,
): Stat {
  if (!v.lastVisitedAt)
    return { label: "Days since", value: "–", note: "No visits yet" };
  const days = daysBetween(v.lastVisitedAt, today);
  return {
    label: "Days since",
    value: formatNumber(days),
    note: days > 14 ? "Time to follow up" : "Seen recently",
  };
}

// Once they are a member, "days since" no longer matters; what matters is when it happened.
export function becameStat(
  v: Pick<Visitor, "convertedAt" | "visitCount">,
): Stat {
  const visits =
    v.visitCount === 1 ? "1 visit" : `${formatNumber(v.visitCount)} visits`;
  return {
    label: "Became member",
    value: v.convertedAt ? formatDate(v.convertedAt) : "–",
    note: `After ${visits}`,
  };
}
