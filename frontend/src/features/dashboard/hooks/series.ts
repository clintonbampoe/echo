import { getPresenceMonths, type Presence } from "../api/attendance";
import { getJoinedMonths } from "../api/members";
import { getTitheMonths, type TitheFigures } from "../api/tithes";
import type { Bucket } from "../model/trend";
import { useMonthly } from "./internals";

export type Series = {
  values: number[] | undefined;
  failed: boolean;
  retry: () => void;
};

const noPresence: Presence = { present: 0, members: 0, visitors: 0 };
const noTithes: TitheFigures = { total: 0, tithers: 0, average: 0 };

// People present in each month, split into members and visitors. One call, whatever the number of months.
export const usePresenceSeries = (buckets: readonly Bucket[]) =>
  useMonthly(
    "presence-months",
    buckets,
    getPresenceMonths,
    (r) => r ?? noPresence,
  );

// Tithes, tithers and the average tithe in each month.
export const useTitheSeries = (buckets: readonly Bucket[]) =>
  useMonthly("tithe-months", buckets, getTitheMonths, (r) => r ?? noTithes);

// New members in each month. Members with no joined date are in no month.
export const useGrowth = (buckets: readonly Bucket[]): Series =>
  useMonthly(
    "growth-months",
    buckets,
    getJoinedMonths,
    (r) => r?.newMembers ?? 0,
  );
