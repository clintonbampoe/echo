// The months Member growth draws, and the figures under them. Pure.
import { monthLength } from "@/lib/dates";
import { formatNumber } from "@/lib/format";
import { monthSlots } from "./money";
import type { Bucket } from "./trend";

// The months ending with this one. This month counts as far as today.
export function growthBuckets(today: string, months: number): Bucket[] {
  return monthSlots(today, months).map((slot) => {
    const [year = 0, m = 1] = slot.key.split("-").map(Number);
    const last = `${slot.key}-${String(monthLength(year, m)).padStart(2, "0")}`;
    return {
      from: `${slot.key}-01`,
      to: slot.key === today.slice(0, 7) ? today : last,
      label: slot.label,
      name: slot.name,
      tip: slot.tip,
    };
  });
}

export type GrowthView = {
  total?: string;
  lastWeek?: string;
  average?: string;
};

// The total over the months, last week's, and the average a month, as text.
export function growthView(
  values: readonly number[] | undefined,
  lastWeek: number | undefined,
): GrowthView {
  const sum = values?.reduce((a, b) => a + b, 0);
  return {
    total: sum === undefined ? undefined : formatNumber(sum),
    average:
      sum === undefined || !values?.length
        ? undefined
        : formatNumber(Math.round(sum / values.length)),
    lastWeek: lastWeek === undefined ? undefined : formatNumber(lastWeek),
  };
}
