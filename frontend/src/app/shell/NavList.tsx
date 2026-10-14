import { NavLink } from "react-router-dom";
import { Icon } from "@/ui";
import { navGroups, type NavItem } from "./nav";
import styles from "./NavList.module.css";

type Props = {
  // Called after a link is followed, so a drawer can close itself.
  onNavigate?: () => void;
};

export function NavList({ onNavigate }: Props) {
  return (
    <nav aria-label="Main" className={styles.nav}>
      {navGroups.map((group, i) => (
        <div key={group.title ?? i}>
          {group.title && <p className={styles.group}>{group.title}</p>}
          {group.items.map((item) => (
            <Item key={item.to} item={item} onNavigate={onNavigate} />
          ))}
        </div>
      ))}
    </nav>
  );
}

function Item({
  item,
  onNavigate,
}: {
  item: NavItem;
  onNavigate?: () => void;
}) {
  if (!item.ready) {
    return (
      <span
        aria-disabled="true"
        title={`${item.label} is coming soon`}
        className={`${styles.item} ${styles.soon}`}
      >
        <Icon name={item.icon} size={18} />
        <span className={styles.label}>{item.label}</span>
        <span className={styles.badge}>Soon</span>
      </span>
    );
  }
  return (
    <NavLink
      to={item.to}
      onClick={onNavigate}
      className={({ isActive }) =>
        `${styles.item} ${isActive ? styles.on : ""}`
      }
    >
      <Icon name={item.icon} size={18} />
      <span className={styles.label}>{item.label}</span>
    </NavLink>
  );
}
