// The weekly money summary a treasurer or pastor can send on: everything that came in, everything that went out, and
// what is left. No React here.
import { daysAfter, weekStart } from "@/lib/dates";
import { formatNumber } from "@/lib/format";
import type { CategoryTotal } from "../api/money";
import type { TitheFigures } from "../api/tithes";

export type WeekKey = "last" | "this";

export const weekOptions = [
  { value: "this", label: "This week" },
  { value: "last", label: "Last week" },
] as const;

// Weeks start on Sunday, as on the server. "This week" runs to today.
export function weekDates(key: WeekKey, today: string) {
  const start = weekStart(today);
  return key === "this"
    ? { from: start, to: today }
    : { from: daysAfter(start, -7), to: daysAfter(start, -1) };
}

export type Row = { name: string; amount: number; note?: string };

export type WeekFigures = {
  came: number;
  cameRows: Row[];
  spent: number;
  spentRows: Row[];
  net: number;
};

// The biggest few, the rest together as Other.
function topRows(rows: Row[], keep: number): Row[] {
  const sorted = [...rows].sort((a, b) => b.amount - a.amount);
  const rest = sorted.slice(keep).reduce((n, r) => n + r.amount, 0);
  const head = sorted.slice(0, keep);
  return rest > 0 ? [...head, { name: "Other", amount: rest }] : head;
}

export function weekFigures(
  categories: readonly CategoryTotal[],
  tithes: TitheFigures,
  gifts: number,
): WeekFigures {
  const sum = (type: CategoryTotal["type"]) =>
    categories.filter((c) => c.type === type).reduce((n, c) => n + c.total, 0);
  const offerings = sum("Income");
  const spent = sum("Expense");
  const came = tithes.total + offerings + gifts;
  const cameRows: Row[] = [
    {
      name: "Tithes",
      amount: tithes.total,
      note:
        tithes.tithers > 0
          ? `${formatNumber(tithes.tithers)} tithers`
          : undefined,
    },
    { name: "Offerings and other", amount: offerings },
    { name: "Project gifts", amount: gifts },
  ];
  const spentRows = topRows(
    categories
      .filter((c) => c.type === "Expense" && c.total > 0)
      .map((c) => ({ name: c.categoryName, amount: c.total })),
    3,
  );
  return { came, cameRows, spent, spentRows, net: came - spent };
}

// "+21.02k" or "−3.2k": the sign always shows.
export const signed = (n: number) =>
  `${n < 0 ? "−" : "+"}${formatNumber(Math.abs(n))}`;

// The message as it is shared: plain words, with the figures in them.
export function weekMessage(key: WeekKey, f: WeekFigures): string {
  const when = key === "last" ? "Last week" : "This week";
  return `${when}: GHS ${formatNumber(f.came)} came in, ${formatNumber(f.spent)} went out, net ${signed(f.net)}. Thank you for giving.`;
}
