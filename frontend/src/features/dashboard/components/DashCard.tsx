import { useId, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { Icon } from "@/ui";
import styles from "./DashCard.module.css";

type Foot = {
  // A line telling plainly why the numbers are what they are, e.g. "All time, not the period".
  note?: string;
  // Where the link goes, as in "See projects".
  to?: string;
  // The link's own words.
  label: string;
};

type Props = {
  title: string;
  // A line under the title; a narrow card has no room for it.
  hint?: string;
  // Where the title leads, e.g. "/attendance". On a phone it shows a small chevron after the words.
  to?: string;
  // A row under the card, with the note and a link to the records. A narrow card has no room for it.
  foot?: Foot;
  // Sits at the right of the heading, e.g. a range switch.
  control?: ReactNode;
  children: ReactNode;
};

// The link a card's heading or foot carries to its records, e.g. "All events →".
export function SeeLink({ to, children }: { to: string; children: string }) {
  return (
    <Link to={to} className={styles.see}>
      {children}
      <Icon name="arrow-right" size={14} className={styles.seeArrow} />
    </Link>
  );
}

export function DashCard({ title, hint, to, foot, control, children }: Props) {
  const id = useId();
  return (
    <section aria-labelledby={id} className={styles.card}>
      <header className={styles.head}>
        <div className={styles.titles}>
          <h2 id={id} className={styles.title}>
            {to ? (
              <Link to={to} className={styles.link}>
                {title}
                <Icon name="chevron-right" size={16} className={styles.go} />
              </Link>
            ) : (
              title
            )}
          </h2>
          {hint && <p className={styles.hint}>{hint}</p>}
        </div>
        {control && <div className={styles.control}>{control}</div>}
      </header>
      <div className={styles.body}>{children}</div>
      {foot && (
        <footer className={styles.foot}>
          {foot.note && <p className={styles.note}>{foot.note}</p>}
          <SeeLink to={foot.to ?? "/"}>{foot.label}</SeeLink>
        </footer>
      )}
    </section>
  );
}
