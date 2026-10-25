import { useEffect, useId, useRef, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { Button } from "./Button";
import { useLeaving } from "./presenceContext";
import { useLeave } from "./usePresence";
import styles from "./ConfirmDialog.module.css";

type Props = {
  title: string;
  // What will happen, in plain words. Plain text or a few short paragraphs.
  children: ReactNode;
  confirmLabel: string;
  cancelLabel: string;
  onConfirm: () => void;
  onCancel: () => void;
  // Keeps the confirm button off until the dialog has what it needs, e.g. a category to move to.
  confirmDisabled?: boolean;
  // True while the action runs: the confirm button spins and both buttons are locked.
  busy?: boolean;
  // Shown when the action fails, so the person can try again or back out.
  error?: string;
  // "danger" makes the confirm button red, for something that removes data. Everything else stays black.
  tone?: "danger" | "neutral";
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
  confirmDisabled,
  busy,
  error,
  tone = "neutral",
}: Props) {
  const titleId = useId();
  const bodyId = useId();
  const safe = useRef<HTMLButtonElement>(null);
  const exit = useLeave(160);
  const leaving = useLeaving();
  const closing = exit.leaving || leaving;
  const cancel = () => exit.leave(onCancel);

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
        className={`${styles.scrim} scrim-fade`}
        data-closing={closing}
        onClick={busy ? undefined : cancel}
        aria-hidden="true"
      />
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={bodyId}
        className={styles.panel}
        data-closing={closing}
        onKeyDown={(e) => e.key === "Escape" && !busy && cancel()}
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
            block
            variant={tone === "danger" ? "danger" : "primary"}
            loading={busy}
            disabled={confirmDisabled}
            onClick={onConfirm}
          >
            {confirmLabel}
          </Button>
          <Button
            ref={safe}
            block
            variant="ghost"
            disabled={busy}
            onClick={cancel}
          >
            {cancelLabel}
          </Button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
