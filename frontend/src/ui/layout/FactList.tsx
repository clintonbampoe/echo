import styles from "./FactList.module.css";

type Props = {
  // Label and value lines. A missing value shows a dash so the rows never jump.
  rows: [label: string, value: string | null | undefined][];
};

// The facts of a record in a panel: a card of label and value lines.
export function FactList({ rows }: Props) {
  return (
    <dl className={styles.card}>
      {rows.map(([label, value]) => (
        <div key={label} className={styles.row}>
          <dt>{label}</dt>
          <dd>{value || "–"}</dd>
        </div>
      ))}
    </dl>
  );
}
