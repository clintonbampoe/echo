import type { CSSProperties, ReactNode } from "react";
import { Link } from "react-router-dom";
import { ActionMenu, type MenuItem } from "./ActionMenu";
import { Avatar } from "./Avatar";
import { cssVars } from "./cssVars";
import styles from "./PersonList.module.css";

export type PersonColumn<T> = {
  header: string;
  cell: (row: T) => ReactNode;
  // A CSS grid width, e.g. 'minmax(110px, 1.1fr)'.
  width: string;
  // Dropped on a mid-size screen, where the sidebar leaves little room.
  low?: boolean;
  // A status chip. On a phone it sits on its own line under the name.
  chip?: boolean;
  // Numbers line up on the right (on a phone the cards read left to right as usual).
  end?: boolean;
};

// `detail` is the line under the name (an email, or "phone · city"); `plain` leaves it out for rows that have
// nothing to put there.
// The name opens a page (`href`) or, for a row that opens a panel on the same page, runs `onOpen`.
export type Person = {
  name: string;
  detail?: string | null;
  plain?: boolean;
} & ({ href: string; onOpen?: never } | { onOpen: () => void; href?: never });

type Props<T> = {
  // Names the list for screen readers, e.g. "Members".
  label: string;
  rows: T[];
  rowKey: (row: T) => string;
  // Who the row is about. The name is the link (or button), and the whole row opens it.
  person: (row: T) => Person;
  columns: PersonColumn<T>[];
  // The ⋯ menu. Leave it out for a row with no actions.
  actions?: (row: T) => MenuItem[];
  // On a phone each row stays one short line (avatar, name, the line under it, status) instead of a card of
  // label and value pairs. The other columns are left out there, so only use it when the line says enough.
  compact?: boolean;
  // What the first column is called. "Name" unless the rows are about something else ("Member" in a list of tithes).
  nameHeader?: string;
};

const NAME = "minmax(220px, 2.4fr)";
const ACTIONS = "36px";

// People as rows: an avatar, a name and email, then the columns the feature chose. A table row on a
// wide screen and a card on a phone, from the same markup, so the layout never depends on the data
// inside it (design rule S1).
export function PersonList<T>({
  label,
  rows,
  rowKey,
  person,
  columns,
  actions,
  nameHeader = "Name",
  compact,
}: Props<T>) {
  const widths = (list: PersonColumn<T>[]) =>
    [NAME, ...list.map((c) => c.width), ACTIONS].join(" ");
  const grid = {
    "--cols": widths(columns),
    "--cols-mid": widths(columns.filter((c) => !c.low)),
  } as CSSProperties;
  return (
    <div className={`${styles.card} ${compact ? styles.compact : ""}`}>
      <div className={styles.head} aria-hidden="true" style={grid}>
        <span>{nameHeader}</span>
        {columns.map((c) => (
          <span
            key={c.header}
            className={
              [c.low && styles.low, c.end && styles.end]
                .filter(Boolean)
                .join(" ") || undefined
            }
          >
            {c.header}
          </span>
        ))}
        <span />
      </div>
      <ul aria-label={label} className={styles.list}>
        {rows.map((row, i) => (
          <Row
            key={rowKey(row)}
            index={i}
            row={row}
            person={person(row)}
            columns={columns}
            items={actions?.(row)}
            grid={grid}
          />
        ))}
      </ul>
    </div>
  );
}

type RowProps<T> = {
  row: T;
  // Where the row is in the list, so rows can arrive one after another.
  index: number;
  person: Person;
  columns: PersonColumn<T>[];
  items?: MenuItem[];
  grid: CSSProperties;
};

function Row<T>({
  row,
  index,
  person: p,
  columns,
  items,
  grid,
}: RowProps<T>) {
  return (
    <li
      className={`${styles.row} stagger`}
      // Only the first screenful is staggered; later rows (Load more) arrive together.
      style={cssVars({ "--i": Math.min(index, 12) }, grid)}
    >
      <div className={styles.who}>
        <Avatar name={p.name} size={36} />
        <div className={styles.names}>
          <p className={styles.name}>
            {p.href !== undefined ? (
              <Link to={p.href} className={styles.link}>
                {p.name}
              </Link>
            ) : (
              <button type="button" className={styles.link} onClick={p.onOpen}>
                {p.name}
              </button>
            )}
          </p>
          {!p.plain && <p className={styles.detail}>{p.detail || "—"}</p>}
        </div>
      </div>
      {columns.map((c) => (
        <div
          key={c.header}
          className={[
            styles.cell,
            c.low && styles.low,
            c.chip && styles.chip,
            c.end && styles.end,
          ]
            .filter(Boolean)
            .join(" ")}
          data-label={c.header}
        >
          {c.cell(row)}
        </div>
      ))}
      <div className={styles.actions}>
        {items && (
          <ActionMenu
            label={`Actions for ${p.name}`}
            title={p.name}
            items={items}
          />
        )}
      </div>
    </li>
  );
}

// Grey rows shaped like PersonList's, with the sheen sweeping across, while the real ones load.
export function PersonListSkeleton({ rows = 6 }: { rows?: number }) {
  return (
    <div className={styles.card} aria-hidden="true">
      <ul className={styles.list}>
        {Array.from({ length: rows }, (_, i) => (
          <li key={i} className={styles.skeletonRow}>
            <span className={`skeleton ${styles.skeletonAvatar}`} />
            <span className={styles.skeletonLines}>
              <span className="skeleton" style={{ width: `${48 - (i % 3) * 8}%`, height: 14 }} />
              <span className="skeleton" style={{ width: `${30 + (i % 4) * 5}%`, height: 11 }} />
            </span>
            <span className={`skeleton ${styles.skeletonTail}`} />
          </li>
        ))}
      </ul>
    </div>
  );
}
