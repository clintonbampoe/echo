// What a person's attendance narrows by, and what its strip shows. Pure, so the tests can pass "today".
import { monthsBefore } from "@/lib/dates";
import { paths } from "@/lib/paths";
import type { AttendanceFilters, CheckIn, HeldSession } from "./api";

export type Period = "all" | "year" | "12m" | "3m";

export const periodOptions: readonly { value: Period; label: string }[] = [
  { value: "all", label: "All time" },
  { value: "year", label: "This year" },
  { value: "12m", label: "Last 12 months" },
  { value: "3m", label: "Last 3 months" },
];

export function filtersFor(
  type: string,
  period: Period,
  today: string,
): AttendanceFilters {
  const range = {
    all: {},
    year: { from: `${today.slice(0, 4)}-01-01`, to: today },
    "12m": { from: monthsBefore(today, 12), to: today },
    "3m": { from: monthsBefore(today, 3), to: today },
  }[period];
  return { sessionLabelId: type || undefined, ...range };
}

export const isNarrowed = (type: string, period: Period) =>
  type !== "" || period !== "all";

// One square on the strip.
export type Slot = { id: string; date: string; name: string; present: boolean };

// The held sessions oldest first (the API sends newest first), each marked with whether they were there.
export function stripSlots(
  sessions: HeldSession[],
  attended: ReadonlySet<string>,
): Slot[] {
  return sessions
    .map((s) => ({
      id: s.id,
      date: s.date,
      name: s.name || s.sessionLabelName,
      present: attended.has(s.id),
    }))
    .reverse();
}

// Where a check-in's person lives: a member's Attendance tab, or the visitor's page. What they were when they came decides it.
export const personHref = (c: Pick<CheckIn, "personKind" | "personId">) =>
  c.personKind === "Member"
    ? paths.member(c.personId, "attendance")
    : paths.visitor(c.personId);
