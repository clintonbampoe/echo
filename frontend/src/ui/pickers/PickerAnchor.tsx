import {
  useEffect,
  useRef,
  type FocusEvent,
  type KeyboardEvent,
  type ReactNode,
} from "react";
import { BottomSheet } from "../overlays/BottomSheet";
import { keepOnScreen } from "../overlays/placement";
import { useFinePointer } from "../hooks/useFinePointer";
import styles from "./PickerAnchor.module.css";

type Props = {
  open: boolean;
  onClose: () => void;
  // Names the panel, e.g. "Date of birth". A sheet shows it as its heading.
  title: string;
  // What opens: a calendar, wheels.
  panel: ReactNode;
  // The trigger. Give it `data-trigger` so focus can come back to it.
  children: ReactNode;
  className?: string;
};

// Holds a trigger and the panel it opens. With a mouse the panel hangs under the trigger as a popover; on a touch
// screen it rises from the bottom. The popover takes focus when it opens, gives it back when it closes, and closes
// on Escape, on a click outside, or when focus tabs away.
export function PickerAnchor({
  open,
  onClose,
  title,
  panel,
  children,
  className,
}: Props) {
  const fine = useFinePointer();
  const wrap = useRef<HTMLDivElement>(null);
  const wasOpen = useRef(false);

  useEffect(() => {
    if (!fine) return;
    if (open)
      wrap.current?.querySelector<HTMLElement>("[data-autofocus]")?.focus();
    else if (wasOpen.current)
      wrap.current?.querySelector<HTMLElement>("[data-trigger]")?.focus();
    wasOpen.current = open;
  }, [open, fine]);

  useEffect(() => {
    if (!open || !fine) return;
    const outside = (e: PointerEvent) => {
      if (e.target instanceof Node && !wrap.current?.contains(e.target))
        onClose();
    };
    document.addEventListener("pointerdown", outside);
    return () => document.removeEventListener("pointerdown", outside);
  }, [open, fine, onClose]);

  function onBlur(e: FocusEvent<HTMLDivElement>) {
    if (
      fine &&
      open &&
      e.relatedTarget instanceof Node &&
      !e.currentTarget.contains(e.relatedTarget)
    )
      onClose();
  }

  function onKeyDown(e: KeyboardEvent) {
    if (e.key !== "Escape" || !open || !fine) return;
    e.stopPropagation();
    onClose();
  }

  return (
    <div
      ref={wrap}
      className={`${styles.wrap} ${className ?? ""}`}
      onBlur={onBlur}
      onKeyDown={onKeyDown}
    >
      {children}
      {fine && open && (
        <div
          ref={keepOnScreen}
          role="dialog"
          aria-label={title}
          className={styles.popover}
        >
          {panel}
        </div>
      )}
      {!fine && (
        <BottomSheet open={open} onClose={onClose} title={title}>
          {panel}
        </BottomSheet>
      )}
    </div>
  );
}
