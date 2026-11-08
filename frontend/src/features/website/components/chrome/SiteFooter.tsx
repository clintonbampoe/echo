import { Logo, PulseLine } from "@/ui";
import styles from "./SiteFooter.module.css";

// Who we are, in one line. Every page is linked from the header.
export function SiteFooter({ flush = false }: { flush?: boolean }) {
  return (
    <footer
      className={flush ? `${styles.footer} ${styles.flush}` : styles.footer}
    >
      <div className={styles.inner}>
        <div className={styles.who}>
          <span className={styles.brand}>
            <Logo size={22} />
            Echo
          </span>
          <PulseLine className={styles.promise} />
        </div>
      </div>
    </footer>
  );
}
