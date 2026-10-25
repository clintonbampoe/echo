import { getPresence, getPresent } from "../api/attendance";
import { getMembers } from "../api/members";
import { getTitheFigures } from "../api/tithes";
import type { Bucket } from "../model/trend";
import { useUndatedMembers } from "./numbers";
import { useBuckets } from "./internals";

export type Series = {
  values: number[] | undefined;
  failed: boolean;
  retry: () => void;
};

// People present in each stretch. The keys match the Present card's, so last week is asked for once.
export const useSeries = (buckets: readonly Bucket[]): Series =>
  useBuckets("present", buckets, getPresent);

// People present in each stretch, split into members and visitors.
export const usePresenceSeries = (buckets: readonly Bucket[]) =>
  useBuckets("presence", buckets, getPresence);

// Tithes, tithers and the average tithe in each stretch.
export const useTitheSeries = (buckets: readonly Bucket[]) =>
  useBuckets("tithe-figures", buckets, getTitheFigures);

// New members in each stretch, without the members who have no joined date.
export function useGrowth(buckets: readonly Bucket[]): Series {
  const series = useBuckets("members", buckets, getMembers);
  const undated = useUndatedMembers();
  const gone = undated.data;
  return {
    values:
      series.values && gone !== undefined
        ? series.values.map((v) => Math.max(0, v - gone))
        : undefined,
    failed: series.failed || undated.failed,
    retry: () => {
      series.retry();
      undated.retry();
    },
  };
}
