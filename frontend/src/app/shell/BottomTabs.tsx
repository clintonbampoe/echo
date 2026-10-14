import { NavLink } from "react-router-dom";
import { Icon } from "@/ui";
import { tabItems } from "./nav";
import styles from "./BottomTabs.module.css";

type Props = { onMore: () => void };

// Phones only: the four most used pages and a "More" button that opens the full menu.
export function BottomTabs({ onMore }: Props) {
  return (
    <nav aria-label="Quick links" className={styles.tabs}>
      {tabItems.map((item) =>
        item.ready ? (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `${styles.tab} ${isActive ? styles.on : ""}`
            }
          >
            <Icon name={item.icon} size={22} />
            <span>{item.label}</span>
          </NavLink>
        ) : (
          <span
            key={item.to}
            aria-disabled="true"
            className={`${styles.tab} ${styles.soon}`}
          >
            <Icon name={item.icon} size={22} />
            <span>{item.label}</span>
          </span>
        ),
      )}
      <button type="button" className={styles.tab} onClick={onMore}>
        <Icon name="menu" size={22} />
        <span>More</span>
      </button>
    </nav>
  );
}
