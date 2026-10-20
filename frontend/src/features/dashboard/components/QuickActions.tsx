import { useId } from "react";
import { Link } from "react-router-dom";
import { Icon } from "@/ui";
import styles from "./QuickActions.module.css";
import { quickActions } from "../quickActions";

// Six shortcuts to the pages that own each record. Every tile is the same size and holds an icon,
// a name and a hint, so nothing moves whichever words are inside. Wide enough, the six sit in one
// row; narrower, the same tiles scroll sideways (see the CSS).
export function QuickActions() {
  const id = useId();
  return (
    <section aria-labelledby={id} className={styles.section}>
      <h2 id={id} className={styles.heading}>
        Quick actions
      </h2>
      <ul className={styles.row}>
        {quickActions.map((a) => (
          <li key={a.key} className={styles.item}>
            <Link
              to={a.to}
              className={
                a.primary ? `${styles.tile} ${styles.first}` : styles.tile
              }
            >
              <span className={styles.icon}>
                <Icon name={a.icon} />
              </span>
              <span className={styles.text}>
                <span className={styles.label}>{a.label}</span>
                <span className={styles.hint}>{a.hint}</span>
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}
