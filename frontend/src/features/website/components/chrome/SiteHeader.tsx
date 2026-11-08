import { useEffect, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";
import { LinkButton, Logo } from "@/ui";
import { NAV } from "../../content/site";
import styles from "./SiteHeader.module.css";

// The top of every website page: the mark, three pages, Sign in and Get started. On a phone the three
// pages fold into a Menu, which opens as a small card under the button.
export function SiteHeader() {
  const signedIn = useIsSignedIn();
  const [open, setOpen] = useState(false);
  // Escape closes the open menu, like tapping outside it does.
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && setOpen(false);
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open]);
  const close = () => setOpen(false);
  return (
    <header className={styles.header}>
      {open && (
        <button
          type="button"
          className={styles.scrim}
          tabIndex={-1}
          aria-label="Close menu"
          onClick={close}
        />
      )}
      <div className={styles.bar}>
        <Link to="/" className={styles.brand} onClick={close}>
          <Logo size={26} progress />
          <span className={styles.name}>Echo</span>
        </Link>
        <div className={styles.right}>
          <nav aria-label="Main" className={styles.nav}>
            {NAV.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `${styles.link} ${isActive ? styles.active : ""}`
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
          {!signedIn && (
            <Link to="/signin" className={styles.signin} onClick={close}>
              Sign in
            </Link>
          )}
          <LinkButton
            to={signedIn ? "/dashboard" : "/signup"}
            pill
            className={styles.cta}
          >
            {signedIn ? "Open Echo" : "Get started"}
          </LinkButton>
          <div className={styles.menu}>
            <button
              type="button"
              className={styles.menuButton}
              aria-expanded={open}
              onClick={() => setOpen(!open)}
            >
              {open ? "Close" : "Menu"}
            </button>
            {open && (
              <div className={styles.panel}>
                {NAV.map((item) => (
                  <Link key={item.to} to={item.to} onClick={close}>
                    {item.label}
                  </Link>
                ))}
                <Link to={signedIn ? "/dashboard" : "/signin"} onClick={close}>
                  {signedIn ? "Open Echo" : "Sign in"}
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
