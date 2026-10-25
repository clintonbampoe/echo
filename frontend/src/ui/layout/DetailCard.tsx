import type { ReactNode } from "react";
import styles from "./DetailCard.module.css";

// A card of titled blocks. A block is either label and value lines (DetailGroup) or anything else (DetailBlock).
export function DetailCard({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <section className={styles.card} aria-label={label} data-profile="details">
      {children}
    </section>
  );
}

type Row = [label: string, value: string | null | undefined];

// Label and value lines. A missing value shows a dash so the rows never jump.
export function DetailGroup({ title, rows }: { title: string; rows: Row[] }) {
  return (
    <DetailBlock title={title}>
      <dl className={styles.list}>
        {rows.map(([label, value]) => (
          <div key={label} className={styles.row}>
            <dt>{label}</dt>
            <dd>{value || "–"}</dd>
          </div>
        ))}
      </dl>
    </DetailBlock>
  );
}

export function DetailBlock({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <section className={styles.group} aria-label={title}>
      <h3 className={styles.title}>{title}</h3>
      {children}
    </section>
  );
}
