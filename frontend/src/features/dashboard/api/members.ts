import { ALL_TIME } from "@/lib/dates";
import { get } from "./client";

// Members who joined in the range. From 2000 to today it is everyone. Members with no joined date are in
// every range, which is why the dashboard takes them off (see useNewMembers).
export const getMembers = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalMembers: number }>("/v1/members/summary", from, to, signal).then(
    (r) => r.totalMembers,
  );

// Every member, and how many are men and women, and how old they are. Not the period. The gender and
// age groups cards share one answer.
export type GenderFigures = { total: number; male: number; female: number };

export type AgeBand = { label: string; count: number };

export type MemberMix = GenderFigures & {
  active: number;
  average: number;
  bands: AgeBand[];
};

export const getMix = (signal?: AbortSignal) =>
  get<{
    totalMembers: number;
    activeMembers: number;
    maleCount: number;
    femaleCount: number;
    averageAge: number;
    ageGroups: { label: string; count: number }[];
  }>("/v1/members/summary", ALL_TIME.from, ALL_TIME.to, signal).then(
    (r): MemberMix => ({
      total: r.totalMembers,
      active: r.activeMembers,
      male: r.maleCount,
      female: r.femaleCount,
      average: r.averageAge,
      bands: r.ageGroups,
    }),
  );
