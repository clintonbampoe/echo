import { useEffect, useId, useRef, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { Icon } from "./Icon";
import { useLeaving } from "./presenceContext";
import { useDragDismiss } from "./useDragDismiss";
import { usePresence } from "./usePresence";
import styles from "./Sheet.module.css";

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  subtitle?: string;
  // The row of buttons at the bottom. It stays in view while the form scrolls.
  footer: ReactNode;
  // Shakes the panel sideways, to say "not yet" (a form with unsaved changes was asked to close).
  shaking?: boolean;
  children: ReactNode;
};

// A panel for a form or a detail. It floats in from the right on a wide screen or tablet and rises from the
// bottom on a phone. Escape or the close button closes it.
// Focus moves in when it opens and goes back to where it was when it closes; the page behind
// does not scroll while it is open.
export function Sheet({
  open,
  onClose,
  title,
  subtitle,
  footer,
  shaking,
  children,
}: Props) {
  const panel = useRef<HTMLDivElement>(null);
  const titleId = useId();
  const presence = usePresence(open, 220);
  const leaving = useLeaving();
  const { mounted } = presence;
  const drag = useDragDismiss(onClose);
  const closing = presence.closing || leaving;

  useEffect(() => {
    if (!open) return;
    const before = document.activeElement;
    const scroll = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    // A field inside may already have asked for focus; only take it if nobody has.
    if (!panel.current?.contains(document.activeElement))
      panel.current?.focus();
    return () => {
      document.body.style.overflow = scroll;
      if (before instanceof HTMLElement) before.focus();
    };
  }, [open]);

  if (!mounted) return null;
  // Rendered on <body>, not where it is used: a page that animates (and so has a transform)
  // would otherwise become the "screen" the fixed panel is measured against.
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
        className={`${styles.panel} slide-in`}
        data-closing={closing}
        data-shaking={shaking || undefined}
        onKeyDown={(e) => e.key === "Escape" && onClose()}
      >
        <span aria-hidden="true" className={styles.grabber} {...drag} />
        <header className={styles.header} {...drag}>
          <div>
            <h2 id={titleId} className={styles.title}>
              {title}
            </h2>
            {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
          </div>
          <button
            type="button"
            aria-label="Close"
            className={styles.close}
            onClick={onClose}
          >
            <Icon name="x" size={20} />
          </button>
        </header>
        <div className={styles.body}>{children}</div>
        <footer className={styles.footer}>{footer}</footer>
      </div>
    </div>,
    document.body,
  );
}
