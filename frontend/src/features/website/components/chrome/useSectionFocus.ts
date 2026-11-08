import { useLayoutEffect, type RefObject } from "react";
import { useLocation } from "react-router-dom";
import { useMedia } from "@/ui";
import { startFocus } from "./focusController";

// Scrolling stays the browser's own, so it is as smooth as the device can make it. What moves with it is
// how much each section is in focus: --f runs from 1 (it is where it should be) down to 0 (a little over half
// a window away), and the section fades by that much (SiteLayout.module.css). The first time a section
// comes into focus its content arrives, piece by piece (arrival.ts). Nothing is held, gated or snapped.
// Off for reduced motion (live), and in browsers that cannot measure (tests).
export function useSectionFocus(main: RefObject<HTMLElement | null>) {
  const { pathname } = useLocation();
  const reduced = useMedia("(prefers-reduced-motion: reduce)", true);
  useLayoutEffect(() => {
    const el = main.current;
    const off =
      reduced || document.documentElement.dataset.reduceMotion === "true";
    if (!el || off) return;
    return startFocus(el);
  }, [main, pathname, reduced]);
}
