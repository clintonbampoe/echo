import type { CSSProperties, ReactNode } from "react";
import { Link } from "react-router-dom";
import { ActionMenu, type MenuItem } from "./ActionMenu";
import { Avatar } from "./Avatar";
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
};

// `plain` leaves out the line under the name, for rows that have nothing to put there (no "No email").
export type Person = {
  name: string;
  email?: string | null;
  href: string;
  plain?: boolean;
};

type Props<T> = {
  // Names the list for screen readers, e.g. "Members".
  label: string;
  rows: T[];
  rowKey: (row: T) => string;
  // Who the row is about. The name is the link, and the whole row opens it.
  person: (row: T) => Person;
  columns: PersonColumn<T>[];
  // The ⋯ menu. Leave it out for a row with no actions.
  actions?: (row: T) => MenuItem[];
};

const NAME = "minmax(220px, 2.4fr)";
const ACTIONS = "32px";

// People as rows: an avatar, a name and email, then the columns the feature chose. A table row on a wide
// screen and a card on a phone, from the same markup, so the layout never depends on the data
// inside it (design rule S1).
export function PersonList<T>({
  label,
  rows,
  rowKey,
  person,
  columns,
  actions,
}: Props<T>) {
  const widths = (list: PersonColumn<T>[]) =>
    [NAME, ...list.map((c) => c.width), ACTIONS].join(" ");
  const grid = {
    "--cols": widths(columns),
    "--cols-mid": widths(columns.filter((c) => !c.low)),
  } as CSSProperties;
  return (
    <div className={styles.card}>
      <div className={styles.head} aria-hidden="true" style={grid}>
        <span>Name</span>
        {columns.map((c) => (
          <span key={c.header} className={c.low ? styles.low : undefined}>
            {c.header}
          </span>
        ))}
        <span />
      </div>
      <ul aria-label={label} className={styles.list}>
        {rows.map((row) => (
          <Row
            key={rowKey(row)}
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
  person: Person;
  columns: PersonColumn<T>[];
  items?: MenuItem[];
  grid: CSSProperties;
};

function Row<T>({ row, person: p, columns, items, grid }: RowProps<T>) {
  return (
    <li className={styles.row} style={grid}>
      <div className={styles.who}>
        <Avatar name={p.name} size={36} />
        <div className={styles.names}>
          <p className={styles.name}>
            <Link to={p.href} className={styles.link}>
              {p.name}
            </Link>
          </p>
          {!p.plain && <p className={styles.email}>{p.email || "No email"}</p>}
        </div>
      </div>
      {columns.map((c) => (
        <div
          key={c.header}
          className={[styles.cell, c.low && styles.low, c.chip && styles.chip]
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
