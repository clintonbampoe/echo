// Lists that load a page at a time, shared by every feature. This is the one place outside a feature's
// hooks.ts that touches TanStack Query, so the same six lines are not copied into each feature.
import {
  useInfiniteQuery,
  type InfiniteData,
  type UseInfiniteQueryResult,
} from "@tanstack/react-query";
import { nextCursor, type Page } from "./api";

// The largest page the API allows: most lists fit on one, and a tab can say "50+" when they do not.
export const PAGE_SIZE = 50;

// What a list on screen needs: the rows so far, and the state around them.
export type Paged<T> = {
  rows: T[];
  loading: boolean;
  failed: boolean;
  hasMore: boolean;
  loadingMore: boolean;
  more: () => void;
  retry: () => void;
};

function paged<T>(q: UseInfiniteQueryResult<InfiniteData<Page<T>>>): Paged<T> {
  return {
    rows: q.data?.pages.flatMap((p) => p.data) ?? [],
    loading: q.isPending,
    failed: q.isError,
    hasMore: q.hasNextPage,
    loadingMore: q.isFetchingNextPage,
    more: () => void q.fetchNextPage(),
    retry: () => void q.refetch(),
  };
}

type Load<T> = (
  cursor: string | undefined,
  signal: AbortSignal,
) => Promise<Page<T>>;

// A list read with a cursor: the same four lines for every endpoint.
export function usePagedList<T>(
  queryKey: readonly unknown[],
  load: Load<T>,
): Paged<T> {
  return paged(
    useInfiniteQuery({
      queryKey,
      queryFn: ({ pageParam, signal }) => load(pageParam, signal),
      initialPageParam: undefined as string | undefined,
      getNextPageParam: nextCursor,
    }),
  );
}

// How many records a tab holds, for its badge. A list loads one page at a time, so when there is
// more we can only say "at least": "50+".
export const countLabel = (loaded: number, hasMore: boolean): string =>
  hasMore ? `${loaded}+` : String(loaded);
