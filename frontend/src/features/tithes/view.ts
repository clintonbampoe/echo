// What a member's tithes look like across a year. Pure, so the tests can pass "today".
import { monthNames } from "@/lib/dates";
import type { TitheMonth } from "./api";

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
