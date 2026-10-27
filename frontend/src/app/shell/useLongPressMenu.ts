import { useEffect } from "react";

// How long a finger rests on a row before its ⋯ menu opens, and how far it may drift before it is a scroll.
const HOLD = 500;
const DRIFT = 10;
// What a row is, and the button in it that opens its menu (every list row has one).
const ROW = "li, tr";
const MENU = 'button[aria-haspopup="menu"]';
// Where the press means something else already: the buttons and fields inside a row.
const OWN = "button, input, select, textarea, [role=switch]";

// On a touch screen, holding a row opens its ⋯ menu (feel spec, Touch and gestures). One listener for the
// whole app: it finds the row under the finger and presses its menu button, so no list builds this itself.
export function useLongPressMenu() {
  useEffect(() => {
    let timer: number | undefined;
    let from: { x: number; y: number } | null = null;
    let fired = 0;

    const cancel = () => {
      window.clearTimeout(timer);
      from = null;
    };
    const onStart = (e: TouchEvent) => {
      cancel();
      const t = e.touches[0];
      const target = e.target instanceof Element ? e.target : null;
      const button = target?.closest(ROW)?.querySelector<HTMLElement>(MENU);
      if (!t || !button || target?.closest(OWN)) return;
      from = { x: t.clientX, y: t.clientY };
      timer = window.setTimeout(() => {
        navigator.vibrate?.(8);
        button.click();
        // After the click above, so that one still reaches the menu button.
        fired = Date.now();
      }, HOLD);
    };
    const onMove = (e: TouchEvent) => {
      const t = e.touches[0];
      if (
        from &&
        t &&
        Math.hypot(t.clientX - from.x, t.clientY - from.y) > DRIFT
      )
        cancel();
    };
    // Letting go after the menu opened must not also open the row, and the browser's own link menu stays shut.
    const recent = () => Date.now() - fired < 800;
    const swallow = (e: Event) => {
      if (recent()) e.preventDefault();
    };
    const swallowClick = (e: Event) => {
      if (!recent()) return;
      e.preventDefault();
      e.stopPropagation();
    };

    document.addEventListener("touchstart", onStart, { passive: true });
    document.addEventListener("touchmove", onMove, { passive: true });
    document.addEventListener("touchend", cancel);
    document.addEventListener("touchcancel", cancel);
    document.addEventListener("scroll", cancel, true);
    document.addEventListener("contextmenu", swallow);
    // Capture phase: before the row's own click handler sees it.
    document.addEventListener("click", swallowClick, true);
    return () => {
      cancel();
      document.removeEventListener("touchstart", onStart);
      document.removeEventListener("touchmove", onMove);
      document.removeEventListener("touchend", cancel);
      document.removeEventListener("touchcancel", cancel);
      document.removeEventListener("scroll", cancel, true);
      document.removeEventListener("contextmenu", swallow);
      document.removeEventListener("click", swallowClick, true);
    };
  }, []);
}
