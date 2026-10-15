// What a member's tithes look like across a year. Pure, so the tests can pass "today".
import { paymentLabel, type PaymentMethod } from "@/features/transactions";
import { monthNames } from "@/lib/dates";
import { formatMoney, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { TitheMonth, TitheSummary } from "./api";

type Month = Pick<TitheMonth, "month" | "totalCollected">;

// How many of the months so far this year had a tithe, e.g. 9 of 10.
export function titheMonths(rows: Month[], today: string) {
  const elapsed = Number(today.slice(5, 7));
  const given = rows.filter(
    (r) => r.month <= elapsed && r.totalCollected > 0,
  ).length;
  return { given, elapsed };
}

// One square a month, January to December: given, not given, or not reached yet.
export type YearSlot = {
  month: number;
  name: string;
  state: "on" | "off" | "future";
};

export function yearSlots(rows: Month[], today: string): YearSlot[] {
  const elapsed = Number(today.slice(5, 7));
  return monthNames.map((name, i) => {
    const month = i + 1;
    const given = rows.some((r) => r.month === month && r.totalCollected > 0);
    return {
      month,
      name,
      state: month > elapsed ? "future" : given ? "on" : "off",
    };
  });
}

// One bar of the "Collected per month" chart. `key` is '2026-10', `label` the short month, `name` the full one.
export type Bar = { key: string; label: string; name: string; value: number };

export const chartBars = (
  rows: Pick<TitheMonth, "year" | "month" | "totalCollected">[],
): Bar[] =>
  rows.map((r) => ({
    key: `${r.year}-${String(r.month).padStart(2, "0")}`,
    label: (monthNames[r.month - 1] ?? "").slice(0, 3),
    name: `${monthNames[r.month - 1] ?? ""} ${r.year}`,
    value: r.totalCollected,
  }));

// "Nov 2025 to Oct 2026": the months the chart covers. Nothing when there are no bars.
export function chartRange(bars: Bar[]): string {
  const first = bars[0];
  const last = bars[bars.length - 1];
  if (!first || !last) return "";
  const short = (b: Bar) => `${b.label} ${b.key.slice(0, 4)}`;
  return `${short(first)} to ${short(last)}`;
}

// "Highest: GH₵ 21,300 in September." Nothing when no month has a tithe.
export function highestMonth(bars: Bar[]): string {
  const top = bars.reduce<Bar | null>(
    (best, b) => (b.value > (best?.value ?? 0) ? b : best),
    null,
  );
  return top
    ? `Highest: ${formatMoney(top.value)} in ${top.name.split(" ")[0]}.`
    : "";
}

// The four numbers above the list. `collected` says which days they cover ("Collected this week").
export function titheStats(
  summary: TitheSummary | undefined,
  activeMembers: number | undefined,
  collected: string,
): Stat[] {
  const s = summary;
  const method = s?.mostUsedPaymentMethod
    ? (paymentLabel[s.mostUsedPaymentMethod as PaymentMethod] ??
      s.mostUsedPaymentMethod)
    : "–";
  return [
    {
      label: "Collected",
      value: s ? formatMoney(s.totalCollected) : "–",
      note: collected,
    },
    {
      label: "Members who gave",
      value: s ? formatNumber(s.uniqueTithers) : "–",
      note:
        activeMembers === undefined
          ? ""
          : `Out of ${formatNumber(activeMembers)} active members`,
    },
    {
      label: "Average per member",
      value: s ? formatMoney(s.averagePerMember) : "–",
      note: "Collected divided by those who gave",
    },
    {
      label: "Most used method",
      value: method,
      note: "Of the tithes this period",
    },
  ];
}
