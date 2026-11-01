import { NavLink } from "react-router-dom";
import { roleLabel, useMe } from "@/features/account";
import { Avatar, Icon } from "@/ui";
import { BrandMark } from "./BrandMark";
import { NavList } from "./NavList";
import styles from "./Sidebar.module.css";

// Wide screens: the logo, the pages, and at the bottom who is signed in, with a way to Settings.
export function Sidebar() {
  const me = useMe().data;
  return (
    <aside className={styles.side} aria-label="Sidebar">
      <BrandMark />
      <NavList />
      <div className={styles.foot}>
        <NavLink to="/me" className={styles.me}>
          <Avatar name={me?.name ?? ""} size={36} />
          <span className={styles.text}>
            <span className={styles.name}>{me ? me.name : "Loading…"}</span>
            <span className={styles.sub}>{me ? roleLabel(me.role) : ""}</span>
          </span>
        </NavLink>
        <NavLink to="/settings" aria-label="Settings" className={styles.gear}>
          <Icon name="settings" size={18} />
        </NavLink>
      </div>
    </aside>
  );
}
