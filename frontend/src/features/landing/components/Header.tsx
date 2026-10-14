import { Link } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";
import { LinkButton, Logo } from "@/ui";
import styles from "./Header.module.css";

export function Header() {
  const signedIn = useIsSignedIn();
  return (
    <header className={`${styles.header} anim-fade`}>
      <div className={styles.inner}>
        <Link to="/" className={styles.brand} aria-label="Echo home">
          <Logo size={36} />
          <span className={styles.name}>Echo</span>
        </Link>
        <nav aria-label="Main" className={styles.nav}>
          <a href="#features" className={`${styles.link} ${styles.hideSmall}`}>
            Features
          </a>
          <a href="#how" className={`${styles.link} ${styles.hideSmall}`}>
            How it works
          </a>
          {signedIn ? (
            <LinkButton to="/dashboard" pill>
              Open Echo
            </LinkButton>
          ) : (
            <>
              <Link to="/signin" className={styles.link}>
                Sign in
              </Link>
              <LinkButton to="/signup" pill>
                Get started
              </LinkButton>
            </>
          )}
        </nav>
      </div>
    </header>
  );
}
