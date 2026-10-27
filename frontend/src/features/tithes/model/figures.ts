// The number about a member's tithes for a profile. Pure, so the tests can check it without a screen.
import { formatDayMonth, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import { titheMonths } from "./display";

export function tithesStat(
  months: { month: number; totalCollected: number }[] | undefined,
  today: string,
): Stat {
  if (!months) return { label: "Tithes", value: "–", note: "" };
  const { given, elapsed } = titheMonths(months, today);
  const total = months.reduce((sum, m) => sum + m.totalCollected, 0);
  return {
    label: "Tithes",
    value: formatNumber(total),
    unit: "GHS",
    note: `${given} of ${elapsed} months`,
  };
}

// When they last gave. `last` is the date of their newest tithe.
export function lastTitheStat(
  last: string | undefined,
  loading: boolean,
): Stat {
  if (!last)
    return {
      label: "Last tithe",
      value: "–",
      note: loading ? "" : "No tithes yet",
    };
  return { label: "Last tithe", value: formatDayMonth(last), note: "" };
}
