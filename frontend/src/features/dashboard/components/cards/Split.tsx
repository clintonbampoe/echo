import type { ReactNode } from "react";
import styles from "./Split.module.css";

// One part of a whole: a name, what it adds up to, and its share when the whole is something to be shared out.
export type Part = {
  key: string;
  name: string;
  // The weight of its segment in the bar.
  value: number;
  text: string;
  // "34%".
  share?: string;
  // Sits between the name and the figures, e.g. an "At risk" chip.
  tag?: ReactNode;
  // Where the name leads, if anywhere.
  href?: string;
};

type Props = { label: string; parts: readonly Part[]; empty: string };

// A bar cut into the parts of a whole, and under it a row for each part with the same shade as its segment:
// the way the boards show where money came from, what each project raised and what the assets are worth.
export function Split({ label, parts, empty }: Props) {
  if (parts.length === 0) return <p className={styles.empty}>{empty}</p>;
  return (
    <div className={styles.split}>
      <div role="img" aria-label={label} className={styles.bar}>
        {parts.map((p, i) => (
          <span
            key={p.key}
            className={styles.segment}
            data-shade={Math.min(i, 4)}
            style={{ flexGrow: Math.max(p.value, 0.0001) }}
          />
        ))}
      </div>
      <ul aria-label={label} className={styles.list}>
        {parts.map((p, i) => (
          <li key={p.key} className={styles.row}>
            <span
              className={styles.swatch}
              data-shade={Math.min(i, 4)}
              aria-hidden="true"
            />
            <span className={styles.name}>{p.name}</span>
            {p.tag}
            <span className={styles.text}>{p.text}</span>
            <span className={styles.share}>{p.share ?? ""}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
