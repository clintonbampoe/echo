import { useEffect, useRef } from "react";

// How much of the screen the keyboard must cover before we call it open (px). Below this it is a toolbar.
const KEYBOARD = 120;

// Put the returned ref on a full-screen overlay to keep it to the part of the screen the person can see. On
// iOS the on-screen keyboard shrinks only the visual viewport, so a `fixed` panel is still sized for the whole
// screen and ends up behind the keyboard. This writes where that visible part is as --vv-top and --vv-height on the overlay (the CSS
// falls back to the whole screen without them), sets data-keyboard while the keyboard is up, and scrolls the
// field being typed in back into view.
export function useVisibleArea(active: boolean) {
  const root = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const view = window.visualViewport;
    const el = root.current;
    if (!active || !view || !el) return;
    const fit = () => {
      // Pinched in: the visual viewport is smaller because of the zoom, not the keyboard.
      if (view.scale > 1.01) return reset();
      const covered = window.innerHeight - view.height - view.offsetTop;
      const keyboard = covered > KEYBOARD;
      el.style.setProperty("--vv-top", `${view.offsetTop}px`);
      el.style.setProperty("--vv-height", `${view.height}px`);
      el.dataset.keyboard = String(keyboard);
      const typing = document.activeElement;
      if (keyboard && typing instanceof HTMLElement && el.contains(typing))
        if (typing.matches("input, textarea"))
          typing.scrollIntoView({ block: "nearest" });
    };
    const reset = () => {
      el.style.removeProperty("--vv-top");
      el.style.removeProperty("--vv-height");
      delete el.dataset.keyboard;
    };
    fit();
    view.addEventListener("resize", fit);
    view.addEventListener("scroll", fit);
    return () => {
      view.removeEventListener("resize", fit);
      view.removeEventListener("scroll", fit);
      reset();
    };
  }, [active]);
  return root;
}
