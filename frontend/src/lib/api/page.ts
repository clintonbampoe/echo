// The date range a list was filtered by. `isDefault` is true when the server chose it
// because the request had no from/to.
export type DateWindow = {
  from: string;
  to: string;
  isDefault: boolean;
};

// Lists are cursor-paged. The response has no total: only whether there is more, and where to resume.
export type Page<T> = {
  data: T[];
  hasMore: boolean;
  next: string | null;
  window?: DateWindow | null;
};

// For useInfiniteQuery: the next page parameter, or undefined when the list has ended.
export function nextCursor<T>(last: Page<T>): string | undefined {
  return last.hasMore && last.next ? last.next : undefined;
}
