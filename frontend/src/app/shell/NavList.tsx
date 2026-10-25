import { NavLink, useLocation } from "react-router-dom";
import { useMe } from "@/features/account";
import { Icon, useSlidingThumb } from "@/ui";
import { adminNav, mainNav, type NavItem } from "./nav";
import styles from "./NavList.module.css";

// The sidebar's links. One raised pill glides to whichever page is open. The Admin group is drawn only for
// administrators.
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
        {mainNav.map((item) => (
          <Item key={item.to} item={item} />
        ))}
      </nav>
      {isAdmin && (
        <nav aria-label="Admin" className={styles.admin}>
          <p className={styles.heading}>Admin</p>
          {adminNav.map((item) => (
            <Item key={item.to} item={item} />
          ))}
        </nav>
      )}
    </div>
  );
}

function Item({ item }: { item: NavItem }) {
  return (
    <NavLink to={item.to} className={styles.item}>
      <Icon name={item.icon} size={18} />
      <span>{item.label}</span>
    </NavLink>
  );
}
