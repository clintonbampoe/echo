import { Link } from "react-router-dom";
import { Logo } from "@/ui";
import { FOOTER } from "../../content/site";
import styles from "./SiteFooter.module.css";

export function SiteFooter() {
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <div className={styles.top}>
          <div className={styles.about}>
            <div className={styles.brand}>
              <Logo size={24} />
              <span className={styles.name}>Echo</span>
            </div>
            <p className={styles.line}>
              Church management that gives the answers back. Free and open
              source.
            </p>
            <p className={styles.made}>Made in Accra, Ghana</p>
          </div>
          <div className={styles.columns}>
            {FOOTER.map((column) => (
              <nav
                key={column.title}
                aria-label={column.title}
                className={styles.column}
              >
                <span className={styles.heading}>{column.title}</span>
                {column.links.map((link) =>
                  link.external ? (
                    <a
                      key={link.label}
                      href={link.to}
                      className={styles.link}
                      target="_blank"
                      rel="noreferrer"
                    >
                      {link.label}
                    </a>
                  ) : (
                    <Link key={link.label} to={link.to} className={styles.link}>
                      {link.label}
                    </Link>
                  ),
                )}
              </nav>
            ))}
          </div>
        </div>
        <div className={styles.ghost} aria-hidden="true">
          Echo
        </div>
      </div>
    </footer>
  );
}
