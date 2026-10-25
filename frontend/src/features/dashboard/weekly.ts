// The weekly summary a treasurer or pastor can send on: what came in, what went out and who came. No React here.
import { daysAfter, weekStart } from "@/lib/dates";
import { formatNumber } from "@/lib/format";
import type { CategoryTotal } from "./api";

export type WeekKey = "last" | "this";

export const weekOptions = [
  { value: "last", label: "Last week" },
  { value: "this", label: "This week" },
] as const;

// Weeks start on Sunday, as on the server. "This week" runs to today.
export function weekDates(key: WeekKey, today: string) {
  const start = weekStart(today);
  return key === "this"
    ? { from: start, to: today }
    : { from: daysAfter(start, -7), to: daysAfter(start, -1) };
}

export type WeekFigures = { received: number; spent: number; present: number };

export function weekFigures(
  categories: readonly CategoryTotal[],
  present: number,
): WeekFigures {
  const sum = (type: CategoryTotal["type"]) =>
    categories.filter((c) => c.type === type).reduce((n, c) => n + c.total, 0);
  return { received: sum("Income"), spent: sum("Expense"), present };
}

// The message as it is shared: plain words, with the figures in them.
export function weekMessage(key: WeekKey, f: WeekFigures): string {
  const when = key === "last" ? "Last week" : "This week";
  const came =
    f.present > 0
      ? ` ${formatNumber(f.present)} ${f.present === 1 ? "person" : "people"} came.`
      : "";
  return `${when} we received GHS ${formatNumber(f.received)} and spent ${formatNumber(f.spent)}.${came} Thank you for giving.`;
}
