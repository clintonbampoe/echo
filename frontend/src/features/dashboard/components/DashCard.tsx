import { useId, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { Icon } from "@/ui";
import styles from "./DashCard.module.css";

type Props = {
  title: string;
  // A line under the title; a narrow card has no room for it.
  hint?: string;
  // Where the title leads, e.g. "/attendance".
  to?: string;
  // Sits at the right of the heading, e.g. a range switch.
  control?: ReactNode;
  // A taller card, for one with figures above its chart.
  tall?: boolean;
  // The tallest, for a month of days.
  taller?: boolean;
  children: ReactNode;
};

export function DashCard({
  title,
  hint,
  to,
  control,
  tall,
  taller,
  children,
}: Props) {
  const id = useId();
  const size = taller ? styles.taller : tall ? styles.tall : "";
  return (
    <section aria-labelledby={id} className={`${styles.card} ${size}`}>
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
    </section>
  );
}
