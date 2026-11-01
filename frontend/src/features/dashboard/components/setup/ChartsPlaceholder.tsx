import { useMedia } from "@/ui";
import styles from "./ChartsPlaceholder.module.css";

// Where the charts go on a church's first day: they would only draw zeros, so say what brings them.
export function ChartsPlaceholder() {
  const phone = useMedia("(max-width: 700px)");
  if (phone)
    return (
      <p className={styles.line}>Charts appear here as you add records.</p>
    );
  return (
    <section aria-label="Charts" className={styles.card}>
      <div>
        <p className={styles.title}>Charts appear here as you add records</p>
        <p className={styles.text}>
          Attendance, giving and member growth all start from the steps above.
        </p>
      </div>
    </section>
  );
}
