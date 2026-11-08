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
export function LinkRows({
  rows,
  large,
}: {
  rows: readonly LinkRow[];
  // Bigger titles, for a page where the rows are the main thing.
  large?: boolean;
}) {
  return (
    <div className={`${styles.rows} ${large ? styles.large : ""}`}>
      {rows.map((row) => (
        <Row key={row.title} row={row} />
      ))}
    </div>
  );
}
