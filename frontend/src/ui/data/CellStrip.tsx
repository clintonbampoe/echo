import type { CSSProperties } from "react";
import { Link } from "react-router-dom";
import { Icon } from "../layout/Icon";
import styles from "./CellStrip.module.css";

// One square. on: filled and ticked. off: empty. future: not reached yet.
export type Cell = {
  id: string;
  // Said aloud and shown on hover, e.g. "Sunday Service, 11 Oct 2026: attended".
  name: string;
  state: "on" | "off" | "future";
  // A shorter text for the hover tooltip, e.g. "Oct 2026" (the screen reader still gets `name`).
  tip?: string;
  // When given, the square opens it.
  href?: string;
  // A letter under the square, e.g. the month's initial.
  caption?: string;
};

type Props = {
  // Names the strip for screen readers.
  label: string;
  title: string;
  summary: string;
  cells: Cell[];
  // How many squares the loading state draws.
  slots: number;
  status: "loading" | "failed" | "ready";
  // The first and last square's date, shown under the strip.
  ends?: [string, string];
};

// A row of squares telling a story at a glance: which of the last sessions someone came to, which
// months of the year they gave. Oldest on the left.
export function CellStrip({
  label,
  title,
  summary,
  cells,
  slots,
  status,
  ends,
}: Props) {
  return (
    <section
      aria-label={label}
      aria-busy={status === "loading"}
      className={styles.strip}
    >
      <header className={styles.head}>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.count}>
          {status === "ready"
            ? summary
            : status === "failed"
              ? "Could not load"
              : ""}
        </p>
      </header>
      {status === "ready" ? (
        <Squares cells={cells} ends={ends} />
      ) : (
        <Bones slots={slots} />
      )}
    </section>
  );
}

function Bones({ slots }: { slots: number }) {
  return (
    <div className={styles.cells} aria-hidden="true">
      {Array.from({ length: slots }, (_, i) => (
        <span
          key={i}
          className={`${styles.cell} ${styles.bone}`}
          style={{ "--i": i } as CSSProperties}
        />
      ))}
    </div>
  );
}

function Square({ cell, index }: { cell: Cell; index: number }) {
  const className = `${styles.cell} ${styles[cell.state]}`;
  const stagger = { "--i": index } as CSSProperties;
  const tick = cell.state === "on" && <Icon name="check" size={14} />;
  if (cell.href)
    return (
      <Link
        to={cell.href}
        className={className}
        title={cell.tip ?? cell.name}
        aria-label={cell.name}
        style={stagger}
      >
        {tick}
      </Link>
    );
  return (
    <span
      className={className}
      title={cell.tip ?? cell.name}
      role="img"
      aria-label={cell.name}
      style={stagger}
    >
      {tick}
    </span>
  );
}

function Squares({ cells, ends }: { cells: Cell[]; ends?: [string, string] }) {
  return (
    <>
      <ul className={styles.cells}>
        {cells.map((c, i) => (
          <li key={c.id}>
            <Square cell={c} index={i} />
            {c.caption && (
              <span className={styles.caption} aria-hidden="true">
                {c.caption}
              </span>
            )}
          </li>
        ))}
      </ul>
      {ends && (
        <p className={styles.ends}>
          <span>{ends[0]}</span>
          <span>{ends[1]}</span>
        </p>
      )}
    </>
  );
}
