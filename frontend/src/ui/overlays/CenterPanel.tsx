import { useEffect, useId, useRef, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { Icon } from "../layout/Icon";
import { useDragDismiss } from "../motion/useDragDismiss";
import { usePresence } from "./usePresence";
import styles from "./CenterPanel.module.css";

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
};

// A short glass panel in the middle of the screen, for a small choice on a touch screen (a dropdown, a wheel, a
// date range): auto height, 12px from the sides, never a bottom sheet. A tap outside, the round close button,
// Escape or a drag down closes it. Focus moves in when it opens and back when it closes.
export function CenterPanel({ open, onClose, title, children }: Props) {
  const panel = useRef<HTMLDivElement>(null);
  const titleId = useId();
  const { mounted, closing } = usePresence(open, 200);
  const drag = useDragDismiss(onClose, "all");

  useEffect(() => {
    if (!open) return;
    const before = document.activeElement;
    panel.current?.focus();
    return () => {
      if (before instanceof HTMLElement) before.focus();
    };
  }, [open]);

  if (!mounted) return null;
  return createPortal(
    <div className={styles.root}>
      <div
        className={`${styles.scrim} glass-scrim`}
        data-closing={closing}
        onClick={onClose}
        aria-hidden="true"
      />
      <div
        ref={panel}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
        className={`${styles.panel} glass-panel glass-in-centre`}
        data-closing={closing}
        onKeyDown={(e) => e.key === "Escape" && onClose()}
      >
        <header className={styles.header} {...drag}>
          <h2 id={titleId} className={styles.title}>
            {title}
          </h2>
          <button
            type="button"
            aria-label="Close"
            className={styles.close}
            onClick={onClose}
          >
            <Icon name="x" size={18} />
          </button>
        </header>
        <div className={styles.body}>{children}</div>
      </div>
    </div>,
    document.body,
  );
}
