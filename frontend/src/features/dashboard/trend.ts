// What the Attendance and Tithes cards draw: which stretches of days to count, and the three numbers beside
// them. Pure, so the tests need no screen.
import {
  daysAfter,
  monthLength,
  monthNames,
  monthsAfter,
  weekStart,
} from "@/lib/dates";
import { formatDayMonth } from "@/lib/format";
import { rangeHint } from "./periods";

export const viewKeys = ["weeks", "half", "year"] as const;
export type ViewKey = (typeof viewKeys)[number];
export type Unit = "week" | "month";

export type View = {
  key: ViewKey;
  label: string;
  short: string;
  // How the dropdown names it.
  range: string;
  unit: Unit;
  count: number;
};

const byKey: Record<ViewKey, View> = {
  weeks: {
    key: "weeks",
    label: "8 weeks",
    short: "8 wks",
    range: "Last 8 weeks",
    unit: "week",
    count: 8,
  },
  half: {
    key: "half",
    label: "6 months",
    short: "6 mo",
    range: "Last 6 months",
    unit: "month",
    count: 6,
  },
  year: {
    key: "year",
    label: "Year",
    short: "Year",
    range: "Last 12 months",
    unit: "month",
    count: 12,
  },
};

export const views: readonly View[] = viewKeys.map((key) => byKey[key]);
export const viewOf = (key: ViewKey): View => byKey[key];

// The words around the numbers, which change with the unit.
export const wordsOf = (unit: Unit) =>
  unit === "week"
    ? {
        hint: "People present, week by week",
        last: "Last week",
        average: "Average per week",
        best: "Best week",
      }
    : {
        hint: "People present, month by month",
        last: "Last month",
        average: "Average per month",
        best: "Best month",
      };

// One bar: the days it counts, its short name under the bar, its name for a screen reader and its tooltip heading.
export type Bucket = {
  from: string;
  to: string;
  label: string;
  name: string;
  tip: string;
};

const shortMonth = (month: number) => (monthNames[month - 1] ?? "").slice(0, 3);

// Whole weeks (Sunday to Saturday) ending with last week. This week is not over, so it is left out.
function weekBuckets(today: string, count: number): Bucket[] {
  const thisWeek = weekStart(today);
  return Array.from({ length: count }, (_, i) => {
    const from = daysAfter(thisWeek, -7 * (count - i));
    const to = daysAfter(from, 6);
    return {
      from,
      to,
      label: formatDayMonth(from),
      name: `Week of ${formatDayMonth(from)}`,
      tip: rangeHint({ from, to }),
    };
  });
}

// Whole months ending with last month, for the same reason.
function monthBuckets(today: string, count: number): Bucket[] {
  const thisMonth = today.slice(0, 7);
  return Array.from({ length: count }, (_, i) => {
    const month = monthsAfter(thisMonth, -(count - i));
    const [year = 0, m = 1] = month.split("-").map(Number);
    const last = String(monthLength(year, m)).padStart(2, "0");
    return {
      from: `${month}-01`,
      to: `${month}-${last}`,
      label: shortMonth(m),
      name: `${monthNames[m - 1] ?? ""} ${year}`,
      tip: `${shortMonth(m)} ${year}`,
    };
  });
}

export const bucketsOf = (view: View, today: string): Bucket[] =>
  view.unit === "week"
    ? weekBuckets(today, view.count)
    : monthBuckets(today, view.count);

export type Summary = { last: number; average: number; best: number };

export function summarize(values: readonly number[]): Summary | undefined {
  if (values.length === 0) return undefined;
  const sum = values.reduce((a, b) => a + b, 0);
  return {
    last: values[values.length - 1] ?? 0,
    average: Math.round(sum / values.length),
    best: Math.max(...values),
  };
}
