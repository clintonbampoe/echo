export interface PagedResponse<T> {
  hasMore: boolean;
  next: string | null;
  data: T[];
}
