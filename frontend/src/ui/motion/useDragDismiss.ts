import { useRef, type PointerEvent } from "react";

// How far the finger must travel before it counts as a drag, the share of the sheet's height that closes it,
// and the speed (px per ms) of a flick that closes it however short the drag.
const START = 6;
const CLOSE_AT = 0.25;
const FLICK = 0.6;

type Drag = {
  el: HTMLElement;
  y: number;
  at: number;
  dy: number;
  speed: number;
  active: boolean;
};

// Lets a bottom sheet be pulled down to close (feel spec, Sheets). Spread the result on the grabber and the
// header: the sheet follows the finger, then closes past a quarter of its height or on a fast flick, and
// otherwise springs back. It uses the `translate` property, so it never fights the sheet's own entrance.
// `when` is the media query where the sheet is a bottom sheet; elsewhere dragging does nothing.
export function useDragDismiss(
  onClose: () => void,
  when = "(max-width: 560px)",
) {
  const drag = useRef<Drag | null>(null);

  const follow = (e: PointerEvent<HTMLElement>) => {
    const d = drag.current;
    if (!d) return;
    const el = d.el;
    const dy = Math.max(0, e.clientY - d.y);
    if (!d.active) {
      if (dy < START) return;
      d.active = true;
      e.currentTarget.setPointerCapture(e.pointerId);
      el.style.transition = "none";
    }
    const dt = Math.max(1, e.timeStamp - d.at);
    d.speed = (dy - d.dy) / dt;
    d.dy = dy;
    d.at = e.timeStamp;
    el.style.translate = `0 ${dy}px`;
  };

  // Back to rest, with a spring unless the person asked for less motion.
  const springBack = (el: HTMLElement) => {
    const calm = window.matchMedia?.(
      "(prefers-reduced-motion: reduce)",
    ).matches;
    el.style.transition = calm
      ? "none"
      : "translate var(--t-spring) var(--ease-spring)";
    el.style.translate = "0 0";
    window.setTimeout(() => {
      el.style.transition = "";
      el.style.translate = "";
    }, 320);
  };

  const release = () => {
    const d = drag.current;
    drag.current = null;
    if (!d?.active) return;
    const el = d.el;
    if (d.dy <= el.offsetHeight * CLOSE_AT && d.speed <= FLICK) {
      springBack(el);
      return;
    }
    onClose();
    // A form with unsaved changes asks first instead of closing: then the sheet comes back up.
    window.setTimeout(() => {
      if (el.isConnected && el.dataset.closing !== "true") springBack(el);
    }, 60);
  };

  return {
    onPointerDown: (e: PointerEvent<HTMLElement>) => {
      if (e.pointerType === "mouse" && e.button !== 0) return;
      if (!window.matchMedia?.(when).matches) return;
      const el = e.currentTarget.closest<HTMLElement>('[role="dialog"]');
      if (!el) return;
      drag.current = {
        el,
        y: e.clientY,
        at: e.timeStamp,
        dy: 0,
        speed: 0,
        active: false,
      };
    },
    onPointerMove: follow,
    onPointerUp: release,
    onPointerCancel: release,
  };
}
