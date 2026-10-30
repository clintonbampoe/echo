import type { ReactNode } from "react";
import { ActionMenu, type MenuItem } from "../overlays/ActionMenu";
import { Icon, type IconName } from "./Icon";
import styles from "./RowCard.module.css";

// A card of rows with hairlines between them: the shape of most lists on a phone and of short lists on a
// wide screen. Each row is an icon tile, a title with one line under it, and something at the end.
export function RowCard({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <ul aria-label={label} className={styles.card}>
      {children}
    </ul>
  );
}

type RowProps = {
  icon: IconName;
  title: string;
  sub: string;
  // True for a row that is switched off: its icon and title go quiet.
  muted?: boolean;
  // A chip, a figure or a button, placed before the menu.
  end?: ReactNode;
  actions?: { label: string; title: string; items: MenuItem[] };
  // Makes the icon and text one button, for a row that opens something.
  onOpen?: () => void;
};

export function Row({
  icon,
  title,
  sub,
  muted,
  end,
  actions,
  onOpen,
}: RowProps) {
  const body = (
    <>
      <span className={`av liquid-av ${styles.tile}`}>
        <Icon name={icon} size={20} />
      </span>
      <div className={styles.text}>
        <p className={styles.title}>{title}</p>
        <p className={styles.sub}>{sub}</p>
      </div>
    </>
  );
  return (
    <li className={`${styles.row} liquid-row ${muted ? styles.muted : ""}`}>
      {onOpen ? (
        <button type="button" className={styles.open} onClick={onOpen}>
          {body}
        </button>
      ) : (
        body
      )}
      {end}
      {actions && (
        <ActionMenu
          label={actions.label}
          title={actions.title}
          items={actions.items}
        />
      )}
    </li>
  );
}
