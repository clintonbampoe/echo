import { useMedia } from "@/ui";
import styles from "./Points.module.css";

// A short list of points. On a phone it folds into one tap-to-open row, so a page of ten tools
// shows ten headlines instead of thirty bullets; elsewhere it is simply the list.
export function Points({ items }: { items: readonly string[] }) {
  const phone = useMedia("(max-width: 599px)");
  const list = (
    <ul className={styles.points}>
      {items.map((point) => (
        <li key={point}>{point}</li>
      ))}
    </ul>
  );
  if (!phone) return list;
  return (
    <details className={styles.fold} data-expand>
      <summary className={styles.summary}>What it covers</summary>
      {list}
    </details>
  );
}
