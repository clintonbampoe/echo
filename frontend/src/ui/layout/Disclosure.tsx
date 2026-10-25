import { useId, useState, type ReactNode } from "react";
import { Icon } from "./Icon";
import styles from "./Disclosure.module.css";

type Props = {
  // The button's words, e.g. "More details".
  title: string;
  // A few words after the title saying what is inside, e.g. "email, address, marital status".
  hint?: string;
  // Opens it and keeps it open, e.g. while a field inside has an error the person must see.
  forceOpen?: boolean;
  children: ReactNode;
};

// A fold that opens and closes smoothly, for the less common part of a form. What is inside stays in the
// page (hidden, not removed) so its fields keep their values and a screen reader can find them once it is open.
export function Disclosure({ title, hint, forceOpen, children }: Props) {
  const [open, setOpen] = useState(false);
  const id = useId();
  const shown = open || !!forceOpen;
  return (
    <div className={styles.wrap}>
      <button
        type="button"
        className={styles.toggle}
        aria-expanded={shown}
        aria-controls={id}
        onClick={() => setOpen(!shown)}
      >
        <Icon name="chevron-down" size={16} className={styles.chevron} />
        <span className={styles.title}>{title}</span>
        {hint && <span className={styles.hint}>{hint}</span>}
      </button>
      <div id={id} className={styles.panel} data-open={shown} inert={!shown}>
        <div className={styles.inner}>{children}</div>
      </div>
    </div>
  );
}
