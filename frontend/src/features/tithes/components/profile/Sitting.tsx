import { formatMoney } from "@/lib/format";
import styles from "./Sitting.module.css";

export type Entry = { id: string; name: string; amount: number };

// What has been recorded since the sheet opened, with the running total, so a long sitting at the
// collection table can be checked against the cash in hand without leaving the sheet.
export function Sitting({ entries }: { entries: Entry[] }) {
  if (entries.length === 0) return null;
  const total = entries.reduce((sum, e) => sum + e.amount, 0);
  return (
    <section aria-label="Recorded in this sitting" className={styles.box}>
      <h3 className={styles.title}>Recorded in this sitting</h3>
      <ul className={styles.list}>
        {entries.map((e) => (
          <li key={e.id} className={styles.row}>
            <span className={styles.name}>{e.name}</span>
            <span className={styles.amount}>{formatMoney(e.amount)}</span>
          </li>
        ))}
      </ul>
      <p className={styles.total}>
        <span>
          {entries.length} {entries.length === 1 ? "tithe" : "tithes"}
        </span>
        <strong>{formatMoney(total)}</strong>
      </p>
    </section>
  );
}
