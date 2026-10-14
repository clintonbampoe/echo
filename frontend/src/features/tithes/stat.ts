// The number about a member's tithes for a profile. Pure, so the tests can check it without a screen.
import { formatMoney } from "@/lib/format";
import type { Stat } from "@/ui";
import { titheMonths } from "./view";

export function tithesStat(
  months: { month: number; totalCollected: number }[] | undefined,
  today: string,
): Stat {
  if (!months) return { label: "Tithes this year", value: "–", note: "" };
  const { given, elapsed } = titheMonths(months, today);
  const total = months.reduce((sum, m) => sum + m.totalCollected, 0);
  return {
    label: "Tithes this year",
    value: formatMoney(total),
    note: `${given} of ${elapsed} months`,
  };
}
