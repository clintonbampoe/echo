import type { ReactNode } from "react";
import { Button } from "./Button";
import { Chip } from "./Chip";
import { Icon } from "./Icon";
import styles from "./PageHeader.module.css";

type Props = {
  title: string;
  // One plain line under the title, only where the page needs it (an admin-only page, say).
  text?: string;
  // A small label before the line, e.g. "Admin only".
  badge?: string;
  // The page's main button, e.g. "Add member". A pill with its name on a wide screen, a round + on a phone.
  add?: { label: string; onClick: () => void };
  // Anything else that sits beside the title, e.g. a second button.
  action?: ReactNode;
};

// The top of a page: its name and the one thing you can do from here.
export function PageHeader({ title, text, badge, add, action }: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.top}>
        <h1 className={styles.title}>{title}</h1>
        {(add || action) && (
          <div className={styles.action}>
            {action}
            {add && (
              <>
                <Button onClick={add.onClick} className={styles.wide}>
                  <Icon name="plus" size={16} />
                  {add.label}
                </Button>
                <button
                  type="button"
                  aria-label={add.label}
                  className={styles.round}
                  onClick={add.onClick}
                >
                  <Icon name="plus" size={20} />
                </button>
              </>
            )}
          </div>
        )}
      </div>
      {(text || badge) && (
        <div className={styles.note}>
          {badge && <Chip>{badge}</Chip>}
          {text && <p className={styles.text}>{text}</p>}
        </div>
      )}
    </header>
  );
}
