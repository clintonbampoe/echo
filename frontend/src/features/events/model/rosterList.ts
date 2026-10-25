import type { Paged } from "@/lib/paged";

// What the two tabs know about reading the lists: still loading, could not be read, and how to try again.
export type RosterState = Pick<Paged<unknown>, "loading" | "failed" | "retry">;

// The rows a tab shows, with the state of the reading, as a list the shared list body understands. Every row is
// already loaded, so there is never more to load.
export const rosterList = <T>(rows: T[], state: RosterState): Paged<T> => ({
  ...state,
  rows,
  hasMore: false,
  loadingMore: false,
  more: () => {},
});
