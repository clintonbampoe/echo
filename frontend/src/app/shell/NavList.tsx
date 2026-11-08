import { NavLink, useLocation } from "react-router-dom";
import { Icon, useSlidingThumb } from "@/ui";
import type { NavItem } from "./nav";
import styles from "./NavList.module.css";

type Props = {
  items: NavItem[];
  // Names the list for screen readers: "Main", or "Admin" for the pages only administrators can open.
  label: string;
};

// A list of the sidebar's links. One raised pill glides to whichever of them is open.
export function NavList({ items, label }: Props) {
  const { pathname } = useLocation();
  const column = useSlidingThumb(pathname);
  return (
    <div ref={column} className={styles.column}>
      <span className={styles.slot} aria-hidden="true">
        <span className={styles.thumb} />
      </span>
      <nav aria-label={label} className={styles.nav}>
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
