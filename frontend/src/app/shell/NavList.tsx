import { NavLink, useLocation } from "react-router-dom";
import { useMe } from "@/features/account";
import { Icon, useSlidingThumb } from "@/ui";
import { adminNav, navGroups, type NavGroup, type NavItem } from "./nav";
import styles from "./NavList.module.css";

// The sidebar's links, in small groups. One raised pill glides to whichever page is open. The Admin group is
// drawn only for administrators.
export function NavList() {
  const isAdmin = useMe().data?.role === "Admin";
  const { pathname } = useLocation();
  const column = useSlidingThumb(pathname);
  return (
    <div ref={column} className={styles.column}>
      <span className={styles.slot} aria-hidden="true">
        <span className={styles.thumb} />
      </span>
      <nav aria-label="Main" className={styles.nav}>
        {navGroups.map((group) => (
          <Group key={group.heading ?? "top"} group={group} />
        ))}
      </nav>
      {isAdmin && (
        <nav aria-label="Admin" className={styles.nav}>
          <Group group={{ heading: "Admin", items: adminNav }} />
        </nav>
      )}
    </div>
  );
}

function Group({ group }: { group: NavGroup }) {
  return (
    <div className={styles.group}>
      {group.heading && <p className={styles.heading}>{group.heading}</p>}
      {group.items.map((item) => (
        <Item key={item.to} item={item} />
      ))}
    </div>
  );
}

function Item({ item }: { item: NavItem }) {
  return (
    <NavLink to={item.to} className={styles.item}>
      <Icon name={item.icon} size={20} />
      <span>{item.label}</span>
    </NavLink>
  );
}
