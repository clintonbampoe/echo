import { Link } from "react-router-dom";
import { Icon, Logo } from "@/ui";
import styles from "./TopBar.module.css";

type Props = { onMenu: () => void };

// Phones only: the menu button on the left, the logo (alone) on the right. The sidebar takes over on
// wider screens. The logo goes to the dashboard, the signed-in app's own home.
export function TopBar({ onMenu }: Props) {
  return (
    <header className={styles.bar}>
      <button
        type="button"
        aria-label="Open menu"
        className={styles.button}
        onClick={onMenu}
      >
        <Icon name="menu" size={20} />
      </button>
      <Link
        to="/dashboard"
        aria-label="Echo dashboard"
        className={styles.brand}
      >
        <Logo size={32} />
      </Link>
    </header>
  );
}
