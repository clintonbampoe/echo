import { ALL_TIME } from "@/lib/dates";
import { get, getMonthly } from "./client";

// Members who joined in the range and in the stretch of the same length before it. Members with no joined date
// are in neither.
export type Joined = { joined: number; before: number };

export const getJoined = (from: string, to: string, signal?: AbortSignal) =>
  get<{ joinedInWindow: number; joinedInPreviousWindow: number }>(
    "/v1/members/summary",
    from,
    to,
    signal,
  ).then((r): Joined => ({
    joined: r.joinedInWindow,
    before: r.joinedInPreviousWindow,
  }));

// New members in each of `months` months ending with the month of `to`.
export const getJoinedMonths = (
  months: number,
  to: string,
  signal?: AbortSignal,
) =>
  getMonthly<{ year: number; month: number; newMembers: number }>(
    "/v1/members/summary/monthly",
    months,
    to,
    signal,
  );

// Every member, and how many are men and women, and how old they are. Not the period. The gender and
// age groups cards share one answer.
export type GenderFigures = { total: number; male: number; female: number };

export type AgeBand = { label: string; count: number };

export type MemberMix = GenderFigures & {
  active: number;
  average: number;
  bands: AgeBand[];
  // Members who joined this calendar year so far, whatever the range asked for.
  joinedThisYear: number;
};

export const getMix = (signal?: AbortSignal) =>
  get<{
    totalMembers: number;
    activeMembers: number;
    maleCount: number;
    femaleCount: number;
    averageAge: number;
    ageGroups: { label: string; count: number }[];
    joinedThisYear: number;
  }>("/v1/members/summary", ALL_TIME.from, ALL_TIME.to, signal).then(
    (r): MemberMix => ({
      total: r.totalMembers,
      active: r.activeMembers,
      male: r.maleCount,
      female: r.femaleCount,
      average: r.averageAge,
      bands: r.ageGroups,
      joinedThisYear: r.joinedThisYear,
    }),
  );
