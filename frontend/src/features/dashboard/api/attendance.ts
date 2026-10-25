import { get } from "./client";

// Check-ins in the range.
export const getPresent = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalPresent: number }>(
    "/v1/attendance/summary",
    from,
    to,
    signal,
  ).then((r) => r.totalPresent);

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
