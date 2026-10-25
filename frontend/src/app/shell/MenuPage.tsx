import { useEffect } from "react";
import { createPortal } from "react-dom";
import { Link, NavLink, useNavigate } from "react-router-dom";
import { roleLabel, useCongregation, useMe } from "@/features/account";
import { useSignOut } from "@/features/auth";
import { Avatar, Icon, usePresence, type IconName } from "@/ui";
import { BrandMark } from "./BrandMark";
import { adminNav, mainNav } from "./nav";
import styles from "./MenuPage.module.css";

type Props = { open: boolean; onClose: () => void };

// Tablets and phones: the menu is a page of its own that covers the screen. An account card, the pages,
// then Settings and Sign out. Choosing anything closes it.
export function MenuPage({ open, onClose }: Props) {
  const { mounted, closing } = usePresence(open, 200);

  useEffect(() => {
    if (!open) return;
    const scroll = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = scroll;
    };
  }, [open]);

  if (!mounted) return null;
  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Menu"
      className={`${styles.page} pop`}
      data-closing={closing}
      onKeyDown={(e) => e.key === "Escape" && onClose()}
    >
      <header className={styles.top}>
        <BrandMark />
        <button
          type="button"
          aria-label="Close menu"
          className={styles.close}
          onClick={onClose}
          autoFocus
        >
          <Icon name="x" size={22} />
        </button>
      </header>
      <div className={styles.body}>
        <Account onClose={onClose} />
        <Pages onClose={onClose} />
        <Actions onClose={onClose} />
      </div>
    </div>,
    document.body,
  );
}

function Account({ onClose }: { onClose: () => void }) {
  const me = useMe().data;
  const church = useCongregation().data;
  return (
    <Link
      to="/me"
      className={`${styles.card} ${styles.account}`}
      onClick={onClose}
    >
      <Avatar name={me?.name ?? ""} size={48} />
      <span className={styles.who}>
        <span className={styles.name}>{me ? me.name : "Loading…"}</span>
        <span className={styles.sub}>
          {me && church ? `${roleLabel(me.role)} · ${church.name}` : ""}
        </span>
      </span>
      <Icon name="chevron-right" size={18} className={styles.chevron} />
    </Link>
  );
}

function Pages({ onClose }: { onClose: () => void }) {
  const isAdmin = useMe().data?.role === "Admin";
  const items = isAdmin ? [...mainNav, ...adminNav] : mainNav;
  return (
    <nav aria-label="Main" className={styles.card}>
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          onClick={onClose}
          className={({ isActive }) =>
            `${styles.row} ${isActive ? styles.on : ""}`
          }
        >
          <Tile icon={item.icon} />
          <span className={styles.label}>{item.label}</span>
          <Icon name="chevron-right" size={18} className={styles.chevron} />
        </NavLink>
      ))}
    </nav>
  );
}

function Actions({ onClose }: { onClose: () => void }) {
  const signOut = useSignOut();
  const navigate = useNavigate();
  // Go to sign-in first, then end the session, so the guard around the app does not redirect twice.
  function leave() {
    onClose();
    navigate("/signin", { replace: true });
    signOut.mutate();
  }
  return (
    <div className={styles.card}>
      <Link to="/settings" className={styles.row} onClick={onClose}>
        <Tile icon="settings" />
        <span className={styles.label}>Settings</span>
      </Link>
      <button
        type="button"
        className={`${styles.row} ${styles.danger}`}
        disabled={signOut.isPending}
        onClick={leave}
      >
        <Tile icon="log-out" />
        <span className={styles.label}>Sign out</span>
      </button>
    </div>
  );
}

const Tile = ({ icon }: { icon: IconName }) => (
  <span className={`av ${styles.tile}`}>
    <Icon name={icon} size={16} />
  </span>
);
