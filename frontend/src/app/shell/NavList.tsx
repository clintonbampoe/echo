import { NavLink } from "react-router-dom";
import { useMe } from "@/features/account";
import { Icon } from "@/ui";
import { adminNav, mainNav, type NavItem } from "./nav";
import styles from "./NavList.module.css";

// The sidebar's links. The Admin group is drawn only for administrators.
export function NavList() {
  const isAdmin = useMe().data?.role === "Admin";
  return (
    <>
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
    </>
  );
}

function Item({ item }: { item: NavItem }) {
  return (
    <NavLink
      to={item.to}
      className={({ isActive }) =>
        `${styles.item} ${isActive ? styles.on : ""}`
      }
    >
      <Icon name={item.icon} size={18} />
      {item.label}
    </NavLink>
  );
}
