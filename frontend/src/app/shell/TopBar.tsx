import { Icon } from "@/ui";
import { BrandMark } from "./BrandMark";
import styles from "./TopBar.module.css";

type Props = { onMenu: () => void };

// Tablets and phones: the logo on the left, the menu button on the right.
export function TopBar({ onMenu }: Props) {
  return (
    <header className={styles.bar}>
      <BrandMark />
      <button
        type="button"
        aria-label="Open menu"
        className={styles.button}
        onClick={onMenu}
      >
        <Icon name="menu" size={22} />
      </button>
    </header>
  );
}
