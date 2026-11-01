import { NavLink, useLocation } from "react-router-dom";
import { useMe } from "@/features/account";
import { Icon, useSlidingThumb } from "@/ui";
import { adminNav, mainNav, type NavItem } from "./nav";
import styles from "./NavList.module.css";

// The sidebar's links, in one list. One raised pill glides to whichever page is open. Invitations is added for
// administrators only.
export function NavList() {
  const isAdmin = useMe().data?.role === "Admin";
  const { pathname } = useLocation();
  const column = useSlidingThumb(pathname);
  const items = isAdmin ? [...mainNav, ...adminNav] : mainNav;
  return (
    <div ref={column} className={styles.column}>
      <span className={styles.slot} aria-hidden="true">
        <span className={styles.thumb} />
      </span>
      <nav aria-label="Main" className={styles.nav}>
        {items.map((item) => (
          <Item key={item.to} item={item} />
        ))}
      </nav>
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
