import { useEffect, useState, type CSSProperties } from "react";
import { Link } from "react-router-dom";
import { LinkButton, Logo, ThemeToggle } from "@/ui";
import styles from "./AuthTopBar.module.css";

// The same pages the website's own header offers.
const LINKS = [
  { to: "/about", label: "About" },
  { to: "/community", label: "Community" },
  { to: "/pricing", label: "Pricing" },
  { to: "/security", label: "Security" },
  { to: "/terms", label: "Terms" },
  { to: "/contact?topic=support", label: "Support Echo" },
] as const;

// The top of an auth screen on a tablet or phone, where there is no side pane: the logo, the light/dark
// button, Get started and Menu, which holds the site's pages. On a phone Get started moves into Menu.
export function AuthTopBar() {
  const [open, setOpen] = useState(false);

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
      <div className={styles.actions}>
        <ThemeToggle />
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
          {LINKS.map((l, i) => (
            <Link
              key={l.to}
              to={l.to}
              className={styles.panelLink}
              style={{ "--i": i } as CSSProperties}
              onClick={() => setOpen(false)}
            >
              {l.label}
            </Link>
          ))}
          <Link
            to="/signup"
            className={`${styles.panelLink} ${styles.panelCta}`}
            style={{ "--i": LINKS.length } as CSSProperties}
            onClick={() => setOpen(false)}
          >
            Get started
          </Link>
        </nav>
      )}
    </header>
  );
}
