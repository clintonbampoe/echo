// The four numbers above the members list, worked out from the summaries the server sends.
// No React in this file.
import { formatNumber } from "@/lib/format";
import type { WindowKey } from "@/lib/windows";
import type { Stat } from "@/ui";
import type { MemberSummary } from "../api/members";

export const percent = (part: number, whole: number) =>
  whole > 0 ? Math.round((part / whole) * 100) : 0;

// "+30%" or "−12%" against an earlier figure; nothing when there is no earlier figure to compare with.
export function changeLabel(now: number, before: number) {
  if (before <= 0) return undefined;
  const change = Math.round(((now - before) / before) * 100);
  if (change === 0) return "No change";
  return `${change > 0 ? "+" : "−"}${Math.abs(change)}%`;
}

const newLabel: Record<WindowKey, string> = {
  year: "New this year",
  "last-year": "New last year",
  all: "All members",
};

const count = (
  s: MemberSummary | undefined,
  pick: (s: MemberSummary) => number,
) => (s ? formatNumber(pick(s)) : "–");

// A share of the people whose gender we know, as "54%".
const share = (
  s: MemberSummary | undefined,
  pick: (s: MemberSummary) => number,
) => (s ? String(percent(pick(s), s.femaleCount + s.maleCount)) : "–");

// `previous` is the same count a year earlier. It is only meaningful for "this year", so only then is it compared.
export function memberFigures(
  window: WindowKey,
  s?: MemberSummary,
  previous?: MemberSummary,
): Stat[] {
  const compare = s && previous && window === "year";
  return [
    {
      label: newLabel[window],
      value: count(s, (x) => x.totalMembers),
      note: s ? `${count(s, (x) => x.activeMembers)} active` : "",
      change: compare
        ? changeLabel(s.totalMembers, previous.totalMembers)
        : undefined,
    },
    {
      label: "Women",
      value: share(s, (x) => x.femaleCount),
      unit: s ? "%" : undefined,
      note: s ? `${count(s, (x) => x.femaleCount)} women` : "",
    },
    {
      label: "Men",
      value: share(s, (x) => x.maleCount),
      unit: s ? "%" : undefined,
      note: s ? `${count(s, (x) => x.maleCount)} men` : "",
    },
    {
      label: "Average age",
      value: s ? String(Math.round(s.averageAge)) : "–",
      note: "Years",
    },
  ];
}
