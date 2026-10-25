import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import type { Paged } from "@/lib/paged";
import { ActionMenu, type MenuItem } from "../overlays/ActionMenu";
import { Avatar } from "../layout/Avatar";
import { ListHeader } from "./ListHeader";
import styles from "./RecordLines.module.css";

// One row of a record list as the boards draw it: something on the left (an icon tile, a date, an avatar), a title
// with a line under it, and something at the right (an amount, a date, a chip).
export type Line = {
  // A node, or a person's name for an avatar.
  lead?: ReactNode;
  title: string;
  sub?: ReactNode;
  end?: ReactNode;
};

type Props<T> = {
  label: string;
  list: Paged<T>;
  rowKey: (row: T) => string;
  line: (row: T) => Line;
  // Where a row leads: a page, or a panel on the same page.
  href?: (row: T) => string;
  onOpen?: (row: T) => void;
  // A ⋯ menu at the right of each row, for what a row can do besides open.
  actions?: (row: T) => { label: string; title: string; items: MenuItem[] };
  // What the count calls a row ("asset" -> "Showing 6 assets").
  noun: [one: string, many: string];
  // What the list holds, and the summary at the right of its header (default: how many rows are loaded).
  title: string;
  aside?: ReactNode;
};

// A card of rows with the count and "Load more" as its last row. The title is the real link or button; its ::after
// stretches over the row, so the whole row opens the record.
export function RecordLines<T>({
  label,
  list,
  rowKey,
  line,
  href,
  onOpen,
  actions,
  noun,
  title,
  aside,
}: Props<T>) {
  const shown = list.rows.length;
  return (
    <div className={styles.card}>
      <ListHeader
        title={title}
        summary={aside ?? `${shown}${list.hasMore ? "+" : ""}`}
      />
      <ul aria-label={label} className={styles.list}>
        {list.rows.map((row, i) => {
          const l = line(row);
          return (
            <li
              key={rowKey(row)}
              className={`${styles.row} liquid-row stagger`}
              style={{ "--i": Math.min(i, 12) } as React.CSSProperties}
            >
              {typeof l.lead === "string" ? (
                <Avatar name={l.lead} size={40} />
              ) : (
                l.lead
              )}
              <div className={styles.text}>
                <p className={styles.title}>
                  {href ? (
                    <Link to={href(row)} className={styles.link}>
                      {l.title}
                    </Link>
                  ) : onOpen ? (
                    <button
                      type="button"
                      className={styles.link}
                      onClick={() => onOpen(row)}
                    >
                      {l.title}
                    </button>
                  ) : (
                    l.title
                  )}
                </p>
                {l.sub && <p className={styles.sub}>{l.sub}</p>}
              </div>
              {(l.end || actions) && (
                <div className={styles.end}>
                  {l.end}
                  {actions && <ActionMenu {...actions(row)} />}
                </div>
              )}
            </li>
          );
        })}
      </ul>
      <footer className={styles.footer}>
        <p role="status">
          {list.hasMore
            ? `Showing ${shown}`
            : `${shown} ${shown === 1 ? noun[0] : noun[1]}`}
        </p>
        {list.hasMore && (
          <button
            type="button"
            className={styles.more}
            disabled={list.loadingMore}
            onClick={list.more}
          >
            Load more
          </button>
        )}
      </footer>
    </div>
  );
}
