import { useEffect, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";
import { LinkButton, Logo, ThemeToggle } from "@/ui";
import { useGlide } from "./useGlide";
import { NAV } from "../../content/site";
import styles from "./SiteHeader.module.css";

export function SiteHeader() {
  const signedIn = useIsSignedIn();
  const [open, setOpen] = useState(false);
  const { navRef, style, onEnter, onLeave } = useGlide();
  // Escape closes the open menu, like tapping outside it does.
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && setOpen(false);
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open]);
  return (
    <header className={styles.header}>
      {open && (
        <button
          type="button"
          className={styles.scrim}
          tabIndex={-1}
          aria-label="Close menu"
          onClick={() => setOpen(false)}
        />
      )}
      <Link to="/" className={styles.brand} onClick={() => setOpen(false)}>
        <Logo size={26} />
        <span className={styles.name}>Echo</span>
      </Link>
      <nav
        ref={navRef}
        aria-label="Main"
        className={`${styles.nav} ${open ? styles.open : ""}`}
        onMouseLeave={onLeave}
        onBlur={onLeave}
      >
        <span className={styles.glide} style={style} aria-hidden="true" />
        {NAV.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `${styles.link} ${isActive ? styles.active : ""}`
            }
            onClick={() => setOpen(false)}
            onMouseEnter={onEnter}
            onFocus={onEnter}
          >
            {item.label}
          </NavLink>
        ))}
        {/* The header hides these on a narrow screen, so the open menu carries them. */}
        {open && (
          <>
            {!signedIn && (
              <Link
                to="/signin"
                className={`${styles.link} ${styles.menuSignin}`}
                onClick={() => setOpen(false)}
              >
                Sign in
              </Link>
            )}
            <LinkButton
              to={signedIn ? "/dashboard" : "/signup"}
              pill
              block
              className={styles.menuCta}
              onClick={() => setOpen(false)}
            >
              {signedIn ? "Open Echo" : "Get started"}
            </LinkButton>
          </>
        )}
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
