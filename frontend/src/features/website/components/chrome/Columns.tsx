import styles from "./Columns.module.css";

type Item = { title: string; text: string };

// A row of short ideas: a rule, a bold title, a sentence.
export function Columns({
  items,
  count = 3,
}: {
  items: readonly Item[];
  count?: 2 | 3;
}) {
  return (
    <ul className={`${styles.columns} ${count === 2 ? styles.two : ""}`}>
      {items.map((item) => (
        <li key={item.title} className={styles.item}>
          <h3 className={styles.title}>{item.title}</h3>
          <p className={styles.text}>{item.text}</p>
        </li>
      ))}
    </ul>
  );
}
