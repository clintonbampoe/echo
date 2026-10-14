import { useEffect, useId, useRef, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { Button } from "./Button";
import styles from "./ConfirmDialog.module.css";

type Props = {
  title: string;
  // What will happen, in plain words. Plain text or a few short paragraphs.
  children: ReactNode;
  confirmLabel: string;
  cancelLabel: string;
  onConfirm: () => void;
  onCancel: () => void;
  // True while the action runs: the confirm button spins and both buttons are locked.
  busy?: boolean;
  // Shown when the action fails, so the person can try again or back out.
  error?: string;
};

// Asks "are you sure?" before something that cannot be easily undone. The safe button is
// focused first, so pressing Enter by accident does not delete anything.
export function ConfirmDialog({
  title,
  children,
  confirmLabel,
  cancelLabel,
  onConfirm,
  onCancel,
  busy,
  error,
}: Props) {
  const titleId = useId();
  const bodyId = useId();
  const safe = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    const before = document.activeElement;
    safe.current?.focus();
    return () => {
      if (before instanceof HTMLElement) before.focus();
    };
  }, []);

  return createPortal(
    <div className={styles.root}>
      <div
        className={`${styles.scrim} anim-fade`}
        onClick={busy ? undefined : onCancel}
        aria-hidden="true"
      />
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={bodyId}
        className={styles.panel}
        onKeyDown={(e) => e.key === "Escape" && !busy && onCancel()}
      >
        <h2 id={titleId} className={styles.title}>
          {title}
        </h2>
        <div id={bodyId} className={styles.body}>
          {children}
        </div>
        {error && (
          <p role="alert" className={styles.error}>
            {error}
          </p>
        )}
        <div className={styles.actions}>
          <Button
            ref={safe}
            variant="secondary"
            disabled={busy}
            onClick={onCancel}
          >
            {cancelLabel}
          </Button>
          <Button loading={busy} onClick={onConfirm} className={styles.danger}>
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
