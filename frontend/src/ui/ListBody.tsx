import type { ReactNode } from "react";
import { formatNumber } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { Alert } from "./Alert";
import { Button } from "./Button";
import styles from "./ListBody.module.css";

type Props<T> = {
  list: Paged<T>;
  // What the list is made of, e.g. ['member', 'members']. The loading, error, empty and count lines all use it.
  noun: [one: string, many: string];
  // Said when there is nothing yet, e.g. "Members you add will appear here."
  emptyText: string;
  // True when a search or filter is narrowing the list, so "nothing" means "nothing matches".
  filtered: boolean;
  onClear: () => void;
  // Draws the rows once there are some.
  children: (rows: T[]) => ReactNode;
};

// Everything under a list page's filters: loading, failed, empty, nothing matching, and then the rows
// with a count and "Load more". The page only says how a row looks.
export function ListBody<T>({
  list,
  noun,
  emptyText,
  filtered,
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
    return filtered ? (
      <div role="status" className={styles.note}>
        <p className={styles.title}>No {many} match</p>
        <p className={styles.text}>
          Try a different search, or a wider time range.
        </p>
        <Button variant="secondary" onClick={onClear}>
          Clear filters
        </Button>
      </div>
    ) : (
      <div role="status" className={styles.note}>
        <p className={styles.title}>No {many} yet</p>
        <p className={styles.text}>{emptyText}</p>
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
