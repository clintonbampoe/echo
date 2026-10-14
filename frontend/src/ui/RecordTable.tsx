import type { CSSProperties, ReactNode } from "react";
import { Link } from "react-router-dom";
import type { Paged } from "@/lib/paged";
import { Alert } from "./Alert";
import { Button } from "./Button";
import { EmptyState } from "./EmptyState";
import type { IconName } from "./Icon";
import { RecordSkeleton } from "./RecordSkeleton";
import styles from "./RecordTable.module.css";

export type Column<T> = {
  header: string;
  cell: (row: T) => ReactNode;
  // Numbers line up on the right.
  end?: boolean;
};

type Props<T> = {
  // Names the table for screen readers, e.g. "Tithes".
  label: string;
  columns: Column<T>[];
  list: Paged<T>;
  rowKey: (row: T) => string;
  // Shown when there are no rows: an icon, what is missing, and where the first one comes from.
  empty: { icon: IconName; title: string; text: string };
  // A page with one table fills the space when it is empty; a table among others stays small.
  large?: boolean;
  // What the count at the bottom calls a row, e.g. "tithe" -> "9 tithes recorded".
  noun: [one: string, many: string];
  // Extra text after the count, e.g. the total given.
  summary?: ReactNode;
  // Where a row leads. When given, the first column is a link and the whole row is clickable.
  href?: (row: T) => string;
  // Keeps the table a fixed height: past about five rows the rows scroll, the heading and footer stay.
  scroll?: boolean;
};

// A list of records. A table on a wide screen; on a phone every row becomes a small card where the
// first column is the title and the others are label and value pairs.
export function RecordTable<T>(props: Props<T>) {
  const { label, list, empty, large } = props;
  if (list.loading)
    return <RecordSkeleton label={label} columns={props.columns.length} />;
  if (list.failed) {
    return (
      <div className={styles.gap}>
        <Alert
          title={`We could not load ${label.toLowerCase()}`}
          action={
            <button type="button" onClick={list.retry}>
              Try again
            </button>
          }
        >
          Check your connection, then try again.
        </Alert>
      </div>
    );
  }
  if (list.rows.length === 0) {
    return (
      <div className={styles.gap}>
        <EmptyState compact={!large} icon={empty.icon} title={empty.title}>
          {empty.text}
        </EmptyState>
      </div>
    );
  }
  return <Rows {...props} />;
}

function Rows<T>({
  label,
  columns,
  list,
  rowKey,
  noun,
  summary,
  href,
  scroll,
}: Props<T>) {
  const [first, ...rest] = columns;
  const cols = { "--cols": columns.length } as CSSProperties;
  const shown = list.rows.length;
  return (
    <div className={styles.wrap}>
      <div className={styles.head} aria-hidden="true" style={cols}>
        {columns.map((c) => (
          <span key={c.header} className={c.end ? styles.end : undefined}>
            {c.header}
          </span>
        ))}
      </div>
      <div
        className={scroll ? styles.scroll : undefined}
        tabIndex={scroll ? 0 : undefined}
        role={scroll ? "region" : undefined}
        aria-label={scroll ? `${label}, scrollable` : undefined}
      >
        <ul aria-label={label} className={styles.list}>
          {list.rows.map((row) => (
            <li
              key={rowKey(row)}
              className={`${styles.row} ${href ? styles.clickable : ""}`}
              style={cols}
            >
              <div className={styles.title}>
                {href ? (
                  <Link to={href(row)} className={styles.link}>
                    {first?.cell(row)}
                  </Link>
                ) : (
                  first?.cell(row)
                )}
              </div>
              {rest.map((c) => (
                <div
                  key={c.header}
                  className={`${styles.cell} ${c.end ? styles.end : ""}`}
                  data-label={c.header}
                >
                  {c.cell(row)}
                </div>
              ))}
            </li>
          ))}
        </ul>
      </div>
      <footer className={styles.footer}>
        <p role="status">
          {list.hasMore
            ? `Showing ${shown}`
            : `${shown} ${shown === 1 ? noun[0] : noun[1]}`}
          {summary}
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
    </div>
  );
}
