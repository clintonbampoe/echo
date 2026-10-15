import type { ReactNode } from "react";
import { formatNumber } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { Alert } from "./Alert";
import { Button } from "./Button";
import { EmptyState } from "./EmptyState";
import type { IconName } from "./Icon";
import styles from "./ListBody.module.css";

type Props<T> = {
  list: Paged<T>;
  // The icon of the empty and nothing-matches cards, the same one the page's menu item wears.
  icon: IconName;
  // What the list is made of, e.g. ['member', 'members']. The loading, error, empty and count lines all use it.
  noun: [one: string, many: string];
  // Said when there is nothing yet, e.g. "Members you add will appear here."
  emptyText: string;
  // The heading of that message, when "No members yet" does not fit (a list that starts on "this week", say).
  emptyTitle?: string;
  // True when a search or filter is narrowing the list, so "nothing" means "nothing matches".
  filtered: boolean;
  // What to suggest when nothing matches, if "a different search, or a wider time range" does not fit this list.
  noMatchText?: string;
  onClear: () => void;
  // Draws the rows once there are some.
  children: (rows: T[]) => ReactNode;
};

// Everything under a list page's filters: loading, failed, empty, nothing matching, and then the rows
// with a count and "Load more". The page only says how a row looks.
export function ListBody<T>({
  list,
  icon,
  noun,
  emptyText,
  emptyTitle,
  filtered,
  noMatchText = "Try a different search, or a wider time range.",
  onClear,
  children,
}: Props<T>) {
  const [one, many] = noun;
  if (list.loading)
    return (
      <p role="status" className={styles.note}>
        Loading {many}…
      </p>
    );
  if (list.failed) {
    return (
      <div className={styles.note}>
        <Alert title={`We could not load your ${many}`}>
          Check your connection and try again.
        </Alert>
        <Button variant="secondary" onClick={list.retry}>
          Try again
        </Button>
      </div>
    );
  }
  if (list.rows.length === 0) {
    return (
      <div className={styles.gap}>
        {filtered ? (
          <EmptyState
            icon="search"
            title={`No ${many} match`}
            action={
              <Button variant="secondary" onClick={onClear}>
                Clear filters
              </Button>
            }
          >
            {noMatchText}
          </EmptyState>
        ) : (
          <EmptyState icon={icon} title={emptyTitle ?? `No ${many} yet`}>
            {emptyText}
          </EmptyState>
        )}
      </div>
    );
  }
  return (
    <>
      {children(list.rows)}
      <footer className={styles.footer}>
        <p role="status">
          Showing {formatNumber(list.rows.length)}{" "}
          {list.rows.length === 1 ? one : many}
        </p>
        {list.hasMore && (
          <Button
            variant="secondary"
            loading={list.loadingMore}
            onClick={list.more}
          >
            Load more
          </Button>
        )}
      </footer>
    </>
  );
}
