import { useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";
import { LinkButton, Logo, ThemeToggle } from "@/ui";
import { NAV } from "../../content/site";
import styles from "./SiteHeader.module.css";

export function SiteHeader() {
  const signedIn = useIsSignedIn();
  const [open, setOpen] = useState(false);
  return (
    <header className={styles.header}>
      <Link to="/" className={styles.brand} onClick={() => setOpen(false)}>
        <Logo size={26} />
        <span className={styles.name}>Echo</span>
      </Link>
      <nav
        aria-label="Main"
        className={`${styles.nav} ${open ? styles.open : ""}`}
      >
        {NAV.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `${styles.link} ${isActive ? styles.active : ""}`
            }
            onClick={() => setOpen(false)}
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
      <div className={styles.actions}>
        <ThemeToggle />
        {signedIn ? (
          <LinkButton to="/dashboard" pill className={styles.cta}>
            Open Echo
          </LinkButton>
        ) : (
          <>
            <Link to="/signin" className={styles.signin}>
              Sign in
            </Link>
            <LinkButton to="/signup" pill className={styles.cta}>
              Get started
            </LinkButton>
          </>
        )}
        <button
          type="button"
          className={styles.menu}
          aria-expanded={open}
          onClick={() => setOpen(!open)}
        >
          {open ? "Close" : "Menu"}
        </button>
      </div>
    </header>
  );
}
