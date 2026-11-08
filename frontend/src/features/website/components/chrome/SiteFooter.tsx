import { Link } from "react-router-dom";
import { Logo, PulseLine } from "@/ui";
import { FOOTER } from "../../content/site";
import styles from "./SiteFooter.module.css";

// Who we are in one line, and the pages people look for. The third link only shows where there is room.
export function SiteFooter() {
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <div className={styles.who}>
          <span className={styles.brand}>
            <Logo size={22} />
            Echo
          </span>
          <PulseLine className={styles.promise} />
        </div>
        <nav aria-label="Footer" className={styles.links}>
          {FOOTER.map((item) => (
            <Link key={item.label} to={item.to} className={styles.link}>
              {item.label}
            </Link>
          ))}
        </nav>
      </div>
    </footer>
  );
}
