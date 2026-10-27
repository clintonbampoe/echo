import type { ReactNode } from "react";
import { formatNumber } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { Alert } from "../feedback/Alert";
import { Button } from "../layout/Button";
import { EmptyState } from "../feedback/EmptyState";
import type { IconName } from "../layout/Icon";
import { PersonListSkeleton } from "./PersonList";
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
  // How the rows are sorted, e.g. "Newest first." When given, the count line says so: "Showing all 5 tithes. Newest first."
  order?: string;
  // What stands in for the rows while they load, when PersonList's grey rows are not the right shape.
  skeleton?: ReactNode;
  // Draws the rows once there are some. With `footerInside` it also gets the count and "Load more" to put at the
  // bottom of its own card, as the Members list does.
  children: (rows: T[], footer: ReactNode) => ReactNode;
  footerInside?: boolean;
};

// "Showing 50 members", or with a sort order "Showing all 5 tithes. Newest first." ("all" once nothing more is coming).
function countLine(
  count: number,
  hasMore: boolean,
  [one, many]: [string, string],
  order?: string,
) {
  const all = order && !hasMore && count > 1 ? "all " : "";
  return `Showing ${all}${formatNumber(count)} ${count === 1 ? one : many}${order ? `. ${order}` : ""}`;
}

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
  order,
  skeleton,
  children,
  footerInside,
}: Props<T>) {
  const many = noun[1];
  if (list.loading)
    return (
      <>
        <p role="status" className={styles.loading}>
          Loading {many}…
        </p>
        {skeleton ?? <PersonListSkeleton />}
      </>
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
  const footer = (
    <ListFooter
      list={list}
      noun={noun}
      order={order}
      inside={Boolean(footerInside)}
    />
  );
  return (
    <>
      {children(list.rows, footer)}
      {!footerInside && footer}
    </>
  );
}

type FooterProps<T> = {
  list: Paged<T>;
  noun: [string, string];
  order?: string;
  inside: boolean;
};

// The count and "Load more": under the list, or as the last row of the list's card.
function ListFooter<T>({ list, noun, order, inside }: FooterProps<T>) {
  const more =
    list.hasMore &&
    (inside ? (
      <button
        type="button"
        className={styles.more}
        disabled={list.loadingMore}
        onClick={list.more}
      >
        Load more
      </button>
    ) : (
      <Button
        variant="secondary"
        loading={list.loadingMore}
        onClick={list.more}
      >
        Load more
      </Button>
    ));
  return (
    <footer className={inside ? styles.inside : styles.footer}>
      <p role="status">
        {countLine(list.rows.length, list.hasMore, noun, order)}
      </p>
      {more}
    </footer>
  );
}
