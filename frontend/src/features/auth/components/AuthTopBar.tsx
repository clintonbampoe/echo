import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { LinkButton, Logo, ThemeToggle } from "@/ui";
import styles from "./AuthTopBar.module.css";

// The same pages the website's own header offers.
const LINKS = [
  { to: "/features", label: "Features" },
  { to: "/docs", label: "Docs" },
  { to: "/roadmap", label: "Roadmap" },
  { to: "/community", label: "Community" },
] as const;

// The top of an auth screen on a tablet or phone, where there is no side pane: the logo, the way back
// to the site and the way in. A tablet lists the site's pages; a phone folds them behind Menu.
export function AuthTopBar() {
  const [open, setOpen] = useState(false);
  const { pathname } = useLocation();
  // Someone already on the sign-in page does not need a link to it.
  const onSignIn = pathname === "/signin";

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && setOpen(false);
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open]);

  return (
    <header className={styles.bar}>
      {open && (
        <button
          type="button"
          className={styles.scrim}
          tabIndex={-1}
          aria-label="Close menu"
          onClick={() => setOpen(false)}
        />
      )}
      <Link to="/" className={styles.brand} aria-label="Echo, back to the site">
        <Logo size={26} />
        <span className={styles.name}>Echo</span>
      </Link>
      <nav className={styles.links} aria-label="Site">
        {LINKS.map((l) => (
          <Link key={l.to} to={l.to} className={styles.link}>
            {l.label}
          </Link>
        ))}
      </nav>
      <div className={styles.actions}>
        <ThemeToggle />
        {!onSignIn && (
          <Link to="/signin" className={styles.signin}>
            Sign in
          </Link>
        )}
        <LinkButton to="/signup" pill className={styles.cta}>
          Get started
        </LinkButton>
        <button
          type="button"
          className={styles.menu}
          aria-expanded={open}
          onClick={() => setOpen(!open)}
        >
          {open ? "Close" : "Menu"}
        </button>
      </div>
      {open && (
        <nav className={styles.panel} aria-label="Site menu">
          {LINKS.map((l) => (
            <Link
              key={l.to}
              to={l.to}
              className={styles.panelLink}
              onClick={() => setOpen(false)}
            >
              {l.label}
            </Link>
          ))}
        </nav>
      )}
    </header>
  );
}
