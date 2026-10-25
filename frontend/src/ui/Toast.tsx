import { useEffect, useState, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { ToastContext, type Slot } from "./toastContext";
import { usePresence } from "./usePresence";
import styles from "./Toast.module.css";

const STAYS_MS = 5000;

// Say what just happened ("Member added") without taking the person out of what they were doing. One toast at a
// time: a new one replaces the last. It stays five seconds, and waits while the pointer or focus is on it. Mount the provider once, high in the tree (the app does).
export function ToastProvider({ children }: { children: ReactNode }) {
  const [slot, setSlot] = useState<Slot>({ toast: null, open: false });
  const { mounted, closing } = usePresence(slot.open, 220);
  const [held, setHeld] = useState(false);
  const id = slot.toast?.id;

  useEffect(() => {
    if (id === undefined || held) return;
    const timer = window.setTimeout(
      () => setSlot((s) => ({ ...s, open: false })),
      STAYS_MS,
    );
    return () => window.clearTimeout(timer);
  }, [id, held]);

  const { toast } = slot;
  return (
    <ToastContext.Provider value={setSlot}>
      {children}
      {mounted &&
        toast &&
        createPortal(
          <div className={styles.dock}>
            <div
              role="status"
              className={styles.toast}
              data-closing={closing}
              onMouseEnter={() => setHeld(true)}
              onMouseLeave={() => setHeld(false)}
              onFocus={() => setHeld(true)}
              onBlur={() => setHeld(false)}
            >
              <span className={styles.message}>{toast.message}</span>
              {toast.action && (
                <button
                  type="button"
                  className={styles.action}
                  onClick={() => {
                    toast.action?.onClick();
                    setSlot((s) => ({ ...s, open: false }));
                  }}
                >
                  {toast.action.label}
                </button>
              )}
            </div>
          </div>,
          document.body,
        )}
    </ToastContext.Provider>
  );
}
