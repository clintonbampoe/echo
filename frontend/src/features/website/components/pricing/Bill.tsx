import { LinkButton } from "@/ui";
import { BILL } from "../../content/pricing";
import styles from "./Bill.module.css";

// The price list, drawn as a bill that comes to nothing.
export function Bill() {
  return (
    <div className={styles.card}>
      <div className={styles.head}>
        <span>Your plan</span>
        <span>Everything</span>
      </div>
      <dl className={styles.rows}>
        {BILL.map((row) => (
          <div key={row.label} className={styles.row}>
            <dt>{row.label}</dt>
            <dd>{row.value}</dd>
          </div>
        ))}
      </dl>
      <div className={styles.total}>
        <span>Total due today</span>
        <span className={styles.sum}>GHS 0</span>
      </div>
      <LinkButton to="/signup" size="lg" pill block className={styles.action}>
        Get started free
      </LinkButton>
      <p className={styles.aside}>No, really.</p>
    </div>
  );
}
