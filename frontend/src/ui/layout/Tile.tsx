import { Icon, type IconName } from "./Icon";
import styles from "./Tile.module.css";

// A 40px rounded square with an icon in it, for the left of a list row.
export function IconTile({ icon }: { icon: IconName }) {
  return (
    <span className={`av ${styles.tile}`} aria-hidden="true">
      <Icon name={icon} size={18} />
    </span>
  );
}

// A date as a small calendar tile: "OCT" over "28". `date` is "2026-10-28" or a full timestamp.
export function DateTile({ date }: { date: string }) {
  const d = new Date(date.length === 10 ? `${date}T00:00:00` : date);
  const month = d.toLocaleString("en-GB", { month: "short" }).toUpperCase();
  return (
    <span className={`av ${styles.tile} ${styles.date}`} aria-hidden="true">
      <span className={styles.month}>{month}</span>
      <span className={styles.day}>{String(d.getDate()).padStart(2, "0")}</span>
    </span>
  );
}
