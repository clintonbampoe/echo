import { useState, type FocusEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useMe } from "@/features/account";
import { useSignOut } from "@/features/auth";
import { Avatar, Icon } from "@/ui";
import styles from "./UserMenu.module.css";

// The signed-in person at the bottom of the sidebar. Opens a small menu with Homepage and Sign out.
export function UserMenu() {
  const me = useMe().data;
  const signOut = useSignOut();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);

  // Go to sign-in first, then end the session. If the session ended first, the guard around
  // the app would send the person to sign-in on its own and then this would send them on again.
  function onSignOut() {
    navigate("/signin", { replace: true });
    signOut.mutate();
  }

  // Focus leaving the whole menu closes it.
  function onFocusOut(e: FocusEvent<HTMLDivElement>) {
    if (!e.currentTarget.contains(e.relatedTarget)) setOpen(false);
  }

  return (
    <div
      className={styles.wrap}
      onBlur={onFocusOut}
      onKeyDown={(e) => e.key === "Escape" && setOpen(false)}
    >
      {open && (
        <div
          role="menu"
          aria-label="Account"
          className={`${styles.menu} anim-fade`}
        >
          <Link to="/" role="menuitem" className={styles.option}>
            Homepage
          </Link>
          <button
            type="button"
            role="menuitem"
            className={styles.option}
            disabled={signOut.isPending}
            onClick={onSignOut}
          >
            Sign out
          </button>
        </div>
      )}
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        className={styles.trigger}
        onClick={() => setOpen(!open)}
      >
        <Avatar name={me?.name ?? ""} />
        <span className={styles.text}>
          <span className={styles.name}>{me ? me.name : "Loading…"}</span>
          <span className={styles.sub}>{me?.emailAddress}</span>
        </span>
        <Icon name="more" size={16} className={styles.more} />
      </button>
    </div>
  );
}
