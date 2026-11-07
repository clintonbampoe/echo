import { useEffect } from "react";

// How many overlays want the page still, and what the page's overflow was before the first of them.
let holders = 0;
let before = "";

// Stops the page behind an overlay from scrolling while `locked`. Overlays stack (a choice list opens inside a
// form), so the page is released only when the last one closes, whatever order they close in.
export function useScrollLock(locked: boolean) {
  useEffect(() => {
    if (!locked) return;
    if (holders++ === 0) {
      before = document.body.style.overflow;
      document.body.style.overflow = "hidden";
    }
    return () => {
      if (--holders === 0) document.body.style.overflow = before;
    };
  }, [locked]);
}
