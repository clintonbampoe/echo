import { useEffect, useRef, type ReactNode } from "react";
import styles from "./Drawer.module.css";

type Props = {
  open: boolean;
  onClose: () => void;
  // Read out by screen readers when the drawer opens.
  label: string;
  children: ReactNode;
};

// A panel that slides in from the left over a dimmed page. Escape or a click outside closes it.
// Focus moves into the panel when it opens and goes back to where it was when it closes.
export function Drawer({ open, onClose, label, children }: Props) {
  const panel = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const before = document.activeElement;
    panel.current?.focus();
    return () => {
      if (before instanceof HTMLElement) before.focus();
    };
  }, [open]);

  if (!open) return null;
  return (
    <div className={styles.root}>
      <div
        className={`${styles.scrim} glass-scrim`}
        onClick={onClose}
        aria-hidden="true"
      />
      <div
        ref={panel}
        role="dialog"
        aria-modal="true"
        aria-label={label}
        tabIndex={-1}
        className={`${styles.panel} glass-panel`}
        onKeyDown={(e) => e.key === "Escape" && onClose()}
      >
        {children}
      </div>
    </div>
  );
}
