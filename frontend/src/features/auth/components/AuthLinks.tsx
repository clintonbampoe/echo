import type { ReactNode } from "react";
import { Icon } from "@/ui";
import { GoLink } from "./GoLink";
import { useSplit } from "./splitMode";
import styles from "./AuthLinks.module.css";

// `hint` is the one line shown over the row on hover or focus: where the link leads.
type Item = { to: string; label: string; hint?: string };

type Props = {
  // The short line over the rows on a wide screen: "New to Echo?".
  heading: string;
  // The first item is the main one: it is the only one shown on a tablet or phone.
  items: Item[];
};

// The other places someone on this screen might want to go. On a wide screen they are numbered
// rows in the side pane; on a narrow one, a single underlined link under the card.
export function AuthLinks({ items, heading }: Props) {
  const split = useSplit();
  if (!split) {
    const first = items[0];
    if (!first) return null;
    return (
      <p className={styles.single}>
        <GoLink to={first.to} className={styles.link}>
          {first.label}
        </GoLink>
      </p>
    );
  }
  return (
    <nav aria-label="Other options" className={styles.pane}>
      <h2 className={styles.heading}>{heading}</h2>
      <ul className={styles.rows}>
        {items.map((item, i) => (
          <li key={item.to} className={styles.item}>
            <GoLink to={item.to} className={styles.row}>
              <span className={styles.number} aria-hidden="true">
                0{i + 1}
              </span>
              <span className={styles.label}>{item.label}</span>
              <span className={styles.arrow} aria-hidden="true">
                <Icon name="arrow-right" size={16} />
              </span>
            </GoLink>
            {item.hint && (
              <span className={styles.preview} aria-hidden="true">
                {item.hint}
              </span>
            )}
          </li>
        ))}
      </ul>
    </nav>
  );
}

// A screen with only one place to go back to.
export function BackLink({
  to = "/signin",
  children = "Back to sign in",
  heading = "Changed your mind?",
}: {
  to?: string;
  children?: ReactNode;
  heading?: string;
}) {
  return (
    <AuthLinks heading={heading} items={[{ to, label: String(children) }]} />
  );
}
