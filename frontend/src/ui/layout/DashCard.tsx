import { useId, type ReactNode } from "react";
import { Link } from "react-router-dom";
import styles from "./DashCard.module.css";

type Props = {
  title: string;
  // Sits at the right of the heading: a range pill, or a few quiet words ("Last 30 days").
  control?: ReactNode;
  children: ReactNode;
};

// The link a card's heading carries to its records, e.g. "View all".
export function SeeLink({ to, children }: { to: string; children: string }) {
  return (
    <Link to={to} className={styles.see}>
      {children}
    </Link>
  );
}

// The shell of every dashboard card: a title, one control at the right, and the body under them. The card
// is as tall as what is in it; the cards of a row are stretched to the same height by the row.
export function DashCard({ title, control, children }: Props) {
  const id = useId();
  return (
    <section aria-labelledby={id} className={styles.card}>
      <header className={styles.head}>
        <h2 id={id} className={styles.title}>
          {title}
        </h2>
        {control && <div className={styles.control}>{control}</div>}
      </header>
      <div className={styles.body}>{children}</div>
    </section>
  );
}

// A control that is only words, e.g. "Last 30 days" or "of goal".
export function Aside({ children }: { children: string }) {
  return <span className={styles.aside}>{children}</span>;
}
