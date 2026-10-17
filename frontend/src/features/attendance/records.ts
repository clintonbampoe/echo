// What the Records tab is narrowed by, and how that becomes what the API is asked. No React in this file.
import { ALL_TIME, monthStart, monthsBefore, weekStart } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { withAny, type Option } from "@/ui";
import type {
  PersonKind,
  RecordsQuery,
  SessionFilters,
  SessionLabel,
  SessionStatus,
} from "./api";

// The days the records cover. The default is this month. `custom` uses the `from` and `to` filters.
export const recordsWindows = [
  "week",
  "month",
  "3m",
  "year",
  "last-year",
  "all",
  "custom",
] as const;
export type RecordsWindow = (typeof recordsWindows)[number];

export const windowOptions: readonly { value: RecordsWindow; label: string }[] =
  [
    { value: "week", label: "This week" },
    { value: "month", label: "This month" },
    { value: "3m", label: "Last 3 months" },
    { value: "year", label: "This year" },
    { value: "last-year", label: "Last year" },
    { value: "all", label: "Any time" },
    { value: "custom", label: "Custom range" },
  ];

const statuses: SessionStatus[] = ["Scheduled", "Completed", "Cancelled"];
const kinds: PersonKind[] = ["Member", "Visitor"];

// `type` is a session type's id. `status` only narrows sessions and `kind` only narrows people.
export const recordFilters = urlFilters(
  {
    name: "",
    type: "",
    status: "",
    kind: "",
    window: "month",
    from: "",
    to: "",
  },
  { status: statuses, kind: kinds, window: recordsWindows },
);

export type RecordValues = typeof recordFilters.empty;

export const statusOptions: readonly Option[] = withAny(
  "Status",
  statuses.map((value) => ({ value, label: value })),
);
export const kindOptions: readonly Option[] = withAny(
  "Kind",
  kinds.map((value) => ({ value, label: value })),
);
export const typeOptions = (labels: readonly SessionLabel[]): Option[] =>
  withAny(
    "Type",
    labels.map((l) => ({ value: String(l.id), label: l.name })),
  );

const isDay = (text: string) => /^\d{4}-\d{2}-\d{2}$/.test(text);

// The first and last day of the window. A custom range with a day missing is open on that side, and one typed the
// wrong way round is turned the right way round.
export function windowDates(
  f: Pick<RecordValues, "window" | "from" | "to">,
  today: string,
): { from: string; to: string } {
  const year = Number(today.slice(0, 4));
  const dates = {
    week: { from: weekStart(today), to: today },
    month: { from: monthStart(today), to: today },
    "3m": { from: monthsBefore(today, 3), to: today },
    year: { from: `${year}-01-01`, to: today },
    "last-year": { from: `${year - 1}-01-01`, to: `${year - 1}-12-31` },
    all: ALL_TIME,
  };
  const window = recordsWindows.find((w) => w === f.window) ?? "month";
  if (window !== "custom") return dates[window];
  const from = isDay(f.from) ? f.from : ALL_TIME.from;
  const to = isDay(f.to) ? f.to : today;
  return from <= to ? { from, to } : { from: to, to: from };
}

const statusOf = (value: string) => statuses.find((s) => s === value);
const kindOf = (value: string) => kinds.find((k) => k === value);

export const toSessionFilters = (
  f: RecordValues,
  today: string,
): SessionFilters => ({
  sessionLabelId: f.type ? Number(f.type) : undefined,
  status: statusOf(f.status),
  name: f.name.trim() || undefined,
  ...windowDates(f, today),
});

export const toCheckInQuery = (
  f: RecordValues,
  today: string,
): RecordsQuery => ({
  sessionLabelId: f.type ? Number(f.type) : undefined,
  kind: kindOf(f.kind),
  name: f.name.trim() || undefined,
  ...windowDates(f, today),
});

// The sessions list and the people list each have a filter the other lacks; the one that does not apply is ignored.
const applicable = (f: RecordValues, people: boolean): RecordValues => ({
  ...f,
  status: people ? "" : f.status,
  kind: people ? f.kind : "",
});

// Filters narrowing the list, not counting the search box (which is always visible).
export function countActive(f: RecordValues, people: boolean): number {
  const a = applicable(f, people);
  return (
    (a.type ? 1 : 0) +
    (a.status ? 1 : 0) +
    (a.kind ? 1 : 0) +
    (a.window === "month" ? 0 : 1)
  );
}

export const isNarrowed = (f: RecordValues, people: boolean) =>
  recordFilters.isFiltered(applicable(f, people));
