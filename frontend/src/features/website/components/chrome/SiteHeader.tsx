import { useEffect, useState } from "react";
import type { CSSProperties } from "react";
import { Link, NavLink } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";
import { LinkButton, Logo, ThemeToggle } from "@/ui";
import { NAV } from "../../content/site";
import styles from "./SiteHeader.module.css";

// The top of every website page: the mark, every page, the light/dark button and Get started. Where the
// pages no longer fit they fold into a Menu, which opens as a small card under the button; on a phone
// Get started moves into that card too.
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
          <ThemeToggle />
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
                {NAV.map((item, i) => (
                  <Link
                    key={item.to}
                    to={item.to}
                    onClick={close}
                    style={{ "--i": i } as CSSProperties}
                  >
                    {item.label}
                  </Link>
                ))}
                <Link
                  to={signedIn ? "/dashboard" : "/signup"}
                  onClick={close}
                  className={styles.panelCta}
                  style={{ "--i": NAV.length } as CSSProperties}
                >
                  {signedIn ? "Open Echo" : "Get started"}
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
