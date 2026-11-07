import { Link } from "react-router-dom";
import { PulseLine } from "@/ui";
import { FOOTER_MAIN, FOOTER_MORE } from "../../content/site";
import styles from "./SiteFooter.module.css";

type Item = { label: string; to: string; external?: boolean };

function FooterLink({ item }: { item: Item }) {
  return item.external ? (
    <a href={item.to} className={styles.link} target="_blank" rel="noreferrer">
      {item.label}
    </a>
  ) : (
    <Link to={item.to} className={styles.link}>
      {item.label}
    </Link>
  );
}

// One line: the promise, and the three pages people look for. The rest sit behind "More".
export function SiteFooter() {
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <div className={styles.top}>
          <PulseLine />
          <nav aria-label="Footer" className={styles.links}>
            {FOOTER_MAIN.map((item) => (
              <FooterLink key={item.label} item={item} />
            ))}
            <details className={styles.more}>
              <summary className={styles.summary}>More</summary>
              <div className={styles.moreList}>
                {FOOTER_MORE.map((item) => (
                  <FooterLink key={item.label} item={item} />
                ))}
              </div>
            </details>
          </nav>
        </div>
        <div className={styles.ghost} aria-hidden="true">
          Echo
        </div>
      </div>
    </footer>
  );
}
