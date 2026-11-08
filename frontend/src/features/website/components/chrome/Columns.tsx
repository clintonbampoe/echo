import { useMedia } from "@/ui";
import styles from "./Columns.module.css";

type Item = { title: string; text: string };

// A row of short ideas: a rule, a bold title, a sentence. On a phone each is a tap-to-open row:
// the titles tell the story and the sentence is there for whoever wants it.
export function Columns({
  items,
  count = 3,
}: {
  items: readonly Item[];
  count?: 2 | 3;
}) {
  const phone = useMedia("(max-width: 599px)");
  return (
    <ul className={`${styles.columns} ${count === 2 ? styles.two : ""}`}>
      {items.map((item) =>
        phone ? (
          <li key={item.title} className={styles.fold}>
            <details data-expand>
              <summary className={styles.summary}>
                <h3 className={styles.title}>{item.title}</h3>
              </summary>
              <p className={styles.text}>{item.text}</p>
            </details>
          </li>
        ) : (
          <li key={item.title} className={styles.item}>
            <h3 className={styles.title}>{item.title}</h3>
            <p className={styles.text}>{item.text}</p>
          </li>
        ),
      )}
    </ul>
  );
}
