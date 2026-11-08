import { Link } from "react-router-dom";
import styles from "./LinkRows.module.css";

export type LinkRow = {
  title: string;
  text: string;
  to: string;
  external?: boolean;
};

function Row({ row }: { row: LinkRow }) {
  const inside = (
    <>
      <span>
        <span className={styles.title}>{row.title}</span>
        <span className={styles.text}>{row.text}</span>
      </span>
      <span className={styles.arrow} aria-hidden="true">
        →
      </span>
    </>
  );
  return row.external ? (
    <a href={row.to} className={styles.row} target="_blank" rel="noreferrer">
      {inside}
    </a>
  ) : (
    <Link to={row.to} className={styles.row}>
      {inside}
    </Link>
  );
}

// Big rows that each go somewhere: a heading, one line, an arrow.
export function LinkRows({ rows }: { rows: readonly LinkRow[] }) {
  return (
    <div className={styles.rows}>
      {rows.map((row) => (
        <Row key={row.title} row={row} />
      ))}
    </div>
  );
}
