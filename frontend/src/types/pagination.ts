export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}
