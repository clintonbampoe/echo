import type { CSSProperties, ReactNode } from "react";
import { Link } from "react-router-dom";
import type { Paged } from "@/lib/paged";
import { ActionMenu, type MenuItem } from "../overlays/ActionMenu";
import { Alert } from "../feedback/Alert";
import { Button } from "../layout/Button";
import { EmptyState } from "../feedback/EmptyState";
import { ListHeader } from "./ListHeader";
import type { IconName } from "../layout/Icon";
import { RecordLines, type Line } from "./RecordLines";
import { RecordSkeleton } from "./RecordSkeleton";
import styles from "./RecordTable.module.css";

export type Column<T> = {
  header: string;
  cell: (row: T) => ReactNode;
  // Numbers line up on the right.
  end?: boolean;
  // On a phone, takes a whole row of the card (a long value, or a bar), not half of one (design rule S4).
  full?: boolean;
  // On a phone, comes after the other values of the card, though it sits earlier in the table (a note after the figures).
  last?: boolean;
};

const cellClass = (c: { end?: boolean; full?: boolean; last?: boolean }) =>
  [
    styles.cell,
    c.end && styles.end,
    c.full && styles.full,
    c.last && styles.last,
  ]
    .filter(Boolean)
    .join(" ");

type Props<T> = {
  // Names the table for screen readers, e.g. "Tithes".
  label: string;
  // What the list holds ("Transactions"), drawn as the card's header, and the summary at its right
  // (default: how many rows are loaded).
  title: string;
  aside?: ReactNode;
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
  // A figure at the right of the footer, e.g. "Total GH₵ 6,400".
  total?: ReactNode;
  // Where a row leads. When given, the first column is a link and the whole row is clickable.
  href?: (row: T) => string;
  // For a row that opens a panel on the same page: the first column is a button, and the whole row is clickable.
  onOpen?: (row: T) => void;
  // The ⋯ menu at the end of each row. It needs `rowName`, which names the row for the menu and for screen readers.
  actions?: (row: T) => MenuItem[];
  rowName?: (row: T) => string;
  // Draws each record as a line of the board's list card (lead, title, line under it, end) instead of a table row.
  // The columns are then only used for the loading shape. `actions` and `rowName` give the line its ⋯ menu.
  line?: (row: T) => Line;
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
  if (props.line)
    return (
      <RecordLines
        label={label}
        title={props.title}
        aside={props.aside}
        list={list}
        rowKey={props.rowKey}
        line={props.line}
        href={props.href}
        onOpen={props.onOpen}
        noun={props.noun}
        actions={lineActions(props)}
      />
    );
  return <Rows {...props} />;
}

// The ⋯ menu of a line, when the table was given actions and a name for each row.
function lineActions<T>({ actions, rowName }: Props<T>) {
  if (!actions || !rowName) return undefined;
  return (row: T) => ({
    label: `Actions for ${rowName(row)}`,
    title: rowName(row),
    items: actions(row),
  });
}

// A table kept to a fixed height is a region the keyboard can scroll, and says so.
const scrolling = (label: string, scroll?: boolean) =>
  scroll
    ? {
        className: styles.scroll,
        tabIndex: 0,
        role: "region",
        "aria-label": `${label}, scrollable`,
      }
    : {};

type RowsProps<T> = Pick<
  Props<T>,
  | "label"
  | "title"
  | "aside"
  | "columns"
  | "list"
  | "rowKey"
  | "noun"
  | "summary"
  | "total"
  | "href"
  | "onOpen"
  | "actions"
  | "rowName"
  | "scroll"
>;

function Rows<T>({
  label,
  title: heading,
  aside,
  columns,
  list,
  rowKey,
  noun,
  summary,
  total,
  href,
  onOpen,
  actions,
  rowName,
  scroll,
}: RowsProps<T>) {
  const cols = { "--cols": columns.length } as CSSProperties;
  const shown = list.rows.length;
  const tail = (actions && styles.tail) || "";
  return (
    <div className={styles.wrap}>
      <ListHeader
        title={heading}
        summary={aside ?? `${shown}${list.hasMore ? "+" : ""}`}
      />
      <div className={`${styles.head} ${tail}`} aria-hidden="true" style={cols}>
        {columns.map((c) => (
          <span key={c.header} className={c.end ? styles.end : undefined}>
            {c.header}
          </span>
        ))}
        {actions && <span />}
      </div>
      <div {...scrolling(label, scroll)}>
        <ul aria-label={label} className={styles.list}>
          {list.rows.map((row) => (
            <Row
              key={rowKey(row)}
              row={row}
              columns={columns}
              cols={cols}
              tail={tail}
              href={href}
              onOpen={onOpen}
              actions={actions}
              rowName={rowName}
            />
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
        {total && <p>{total}</p>}
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

type RowProps<T> = Pick<
  Props<T>,
  "columns" | "href" | "onOpen" | "actions" | "rowName"
> & { row: T; cols: CSSProperties; tail: string };

// One record: its first column (a link or a button when the row leads somewhere), the other cells, and the ⋯ menu.
function Row<T>({
  row,
  columns,
  cols,
  tail,
  href,
  onOpen,
  actions,
  rowName,
}: RowProps<T>) {
  const [first, ...rest] = columns;
  const leads = Boolean(href || onOpen);
  const title = first?.cell(row);
  const name = rowName?.(row) ?? "";
  return (
    <li
      className={`${styles.row} liquid-row ${leads ? styles.clickable : ""} ${tail}`}
      style={cols}
    >
      <div className={styles.title}>
        {href ? (
          <Link to={href(row)} className={styles.link}>
            {title}
          </Link>
        ) : onOpen ? (
          <button
            type="button"
            className={styles.link}
            onClick={() => onOpen(row)}
          >
            {title}
          </button>
        ) : (
          title
        )}
      </div>
      {rest.map((c) => (
        <div key={c.header} className={cellClass(c)} data-label={c.header}>
          {c.cell(row)}
        </div>
      ))}
      {actions && (
        <div className={styles.actions}>
          <ActionMenu
            label={`Actions for ${name}`}
            title={name}
            items={actions(row)}
          />
        </div>
      )}
    </li>
  );
}
