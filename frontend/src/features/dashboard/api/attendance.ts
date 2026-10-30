import { get, getMonthly } from "./client";

// Check-ins in the range, and in the stretch of the same length before it: one request for both.
export type Present = { present: number; before: number };

export const getPresent = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalPresent: number; previousTotalPresent: number }>(
    "/v1/attendance/summary",
    from,
    to,
    signal,
    { includePrevious: true },
  ).then((r): Present => ({
    present: r.totalPresent,
    before: r.previousTotalPresent,
  }));

// Who was present in the range: everyone, and the members and visitors among them.
export type Presence = { present: number; members: number; visitors: number };

export const getPresence = (from: string, to: string, signal?: AbortSignal) =>
  get<{
    totalPresent: number;
    membersPresent: number;
    visitorsPresent: number;
  }>("/v1/attendance/summary", from, to, signal).then((r): Presence => ({
    present: r.totalPresent,
    members: r.membersPresent,
    visitors: r.visitorsPresent,
  }));

// Who was present in each of `months` months ending with the month of `to`.
export type PresenceMonth = Presence & { year: number; month: number };

export const getPresenceMonths = (
  months: number,
  to: string,
  signal?: AbortSignal,
) =>
  getMonthly<{
    year: number;
    month: number;
    totalPresent: number;
    membersPresent: number;
    visitorsPresent: number;
  }>("/v1/attendance/summary/monthly", months, to, signal).then((rows) =>
    rows.map((r): PresenceMonth => ({
      year: r.year,
      month: r.month,
      present: r.totalPresent,
      members: r.membersPresent,
      visitors: r.visitorsPresent,
    })),
  );
