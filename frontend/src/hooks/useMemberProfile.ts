import { useInfiniteQuery } from "@tanstack/react-query";
import type { PagedResponse } from "../types/pagination";

/**
 * Cursor-paginated list of one kind of activity for a member.
 * `rootKey` is the entity's existing query key (e.g. "tithes") so the
 * entity's create/update/delete invalidations also refresh the profile.
 */
export const useMemberActivity = <T>(
  rootKey: string,
  memberId: string,
  fetchPage: (memberId: string, cursor?: string) => Promise<PagedResponse<T>>,
) => {
  return useInfiniteQuery({
    queryKey: [rootKey, "memberProfile", memberId],
    queryFn: ({ pageParam }) => fetchPage(memberId, pageParam),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? (lastPage.next ?? undefined) : undefined,
    enabled: !!memberId,
  });
};
