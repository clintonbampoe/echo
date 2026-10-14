import { Link } from "react-router-dom";
import styles from "./Footer.module.css";

export function Footer() {
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <p className={styles.note}>Echo. Made in Accra.</p>
        <nav aria-label="Footer" className={styles.links}>
          <Link to="/signin" className={styles.link}>
            Sign in
          </Link>
          <Link to="/join" className={styles.link}>
            Join with invitation
          </Link>
        </nav>
      </div>
    </footer>
  );
}
