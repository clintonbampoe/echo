import { NavLink } from "react-router-dom";
import { roleLabel, useMe } from "@/features/account";
import { Avatar, Icon } from "@/ui";
import { BrandMark } from "./BrandMark";
import { NavList } from "./NavList";
import { adminNav, mainNav } from "./nav";
import styles from "./Sidebar.module.css";

// Wide screens: the logo and the pages. At the bottom, above who is signed in, come the pages only administrators can
// open, under a small "Admin" label so they can tell those apart. Settings is beside the signed-in person.
export function Sidebar() {
  const me = useMe().data;
  const isAdmin = me?.role === "Admin";
  return (
    <aside className={styles.side} aria-label="Sidebar">
      <BrandMark />
      <NavList items={mainNav} label="Main" />
      {isAdmin && (
        <section className={styles.admin} aria-label="Admin pages">
          <span className={styles.adminLabel}>Admin</span>
          <NavList items={adminNav} label="Admin" />
        </section>
      )}
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
