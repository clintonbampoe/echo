import { useEffect, useId, useRef, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { usePresence } from "./usePresence";
import styles from "./BottomSheet.module.css";

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
};

// A short panel that rises from the bottom of the screen, for a small choice on a touch screen (design
// rule S3). Escape or a tap outside closes it. Focus moves in when it opens and back when it closes.
export function BottomSheet({ open, onClose, title, children }: Props) {
  const panel = useRef<HTMLDivElement>(null);
  const titleId = useId();
  const { mounted, closing } = usePresence(open, 200);

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
        className={`${styles.scrim} scrim-fade`}
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
        className={`${styles.panel} sheet-rise`}
        data-closing={closing}
        onKeyDown={(e) => e.key === "Escape" && onClose()}
      >
        <span className={styles.handle} aria-hidden="true" />
        <h2 id={titleId} className={styles.title}>
          {title}
        </h2>
        {children}
      </div>
    </div>,
    document.body,
  );
}
