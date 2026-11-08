import { useEffect, type RefObject } from "react";
import { useLocation } from "react-router-dom";
import { stepSections } from "./stepSections";

// How long the page holds still once a new section has settled, so its entrance can finish.
const HOLD_MS = 400;
// A scroll counts as stopped after this long without a scroll event.
const SETTLE_MS = 120;
// A section whose top is within this many pixels of the window's top is lined up.
const LINED_UP = 24;

const FIELD = "input, textarea, select, [contenteditable='true']";

function wantsStillness() {
  return (
    typeof matchMedia !== "function" ||
    matchMedia("(prefers-reduced-motion: reduce)").matches ||
    document.documentElement.dataset.reduceMotion === "true"
  );
}

// One section is in focus at a time: the others dim and blur (data-focus on each child of `main`).
// With a mouse or trackpad the page moves a section at a time (stepSections); a touch screen snaps
// (SiteLayout.module.css). A section seen for the first time holds the page still for a moment once it
// has settled, so the reader sees its entrance before moving on. Never when someone prefers less
// motion, when the section is taller than the window (it has to be read by scrolling), or while they
// are typing in a field.
export function useSectionFocus(main: RefObject<HTMLElement | null>) {
  const { pathname } = useLocation();
  useEffect(() => {
    const el = main.current;
    if (!el || wantsStillness()) return;
    const root = document.documentElement;
    const seen = new WeakSet<Element>();
    let frame = 0;
    let settle = 0;
    let hold = 0;
    let moving = 0;
    let first = true;
    let holding = false;

    const sections = () => Array.from(el.children) as HTMLElement[];
    const tall = (r: DOMRect) => r.height > innerHeight * 1.05;
    const focused = (r: DOMRect) =>
      tall(r)
        ? r.top < innerHeight && r.bottom > 0
        : Math.abs(r.top) < innerHeight * 0.45;

    function update() {
      frame = 0;
      for (const s of sections()) {
        s.dataset.focus = focused(s.getBoundingClientRect()) ? "on" : "off";
      }
    }
    function stopped() {
      clearTimeout(moving);
      moving = 0;
      update();
      for (const s of sections()) {
        if (s.dataset.focus !== "on" || seen.has(s)) continue;
        seen.add(s);
        const r = s.getBoundingClientRect();
        const busy = document.activeElement?.matches(FIELD);
        // The first screen needs no hold: nobody has scrolled yet.
        if (first || tall(r) || busy || Math.abs(r.top) > LINED_UP) continue;
        root.dataset.hold = "";
        holding = true;
        clearTimeout(hold);
        hold = window.setTimeout(() => {
          holding = false;
          delete root.dataset.hold;
        }, HOLD_MS);
      }
      first = false;
    }
    const onScroll = () => {
      frame ||= requestAnimationFrame(update);
      clearTimeout(settle);
      settle = window.setTimeout(stopped, SETTLE_MS);
    };

    // Pages load in as the route changes, so look again when the sections do.
    const mutations = new MutationObserver(() => {
      first = true;
      onScroll();
    });
    mutations.observe(el, { childList: true });
    const unstep = matchMedia("(pointer: fine)").matches
      ? stepSections(el, {
          busy: () => holding || moving !== 0,
          // The failsafe frees the page if the scroll never settles.
          start: () => {
            moving = window.setTimeout(() => (moving = 0), 2000);
          },
        })
      : undefined;
    addEventListener("scroll", onScroll, { passive: true });
    addEventListener("resize", onScroll);
    onScroll();
    return () => {
      cancelAnimationFrame(frame);
      clearTimeout(settle);
      clearTimeout(hold);
      clearTimeout(moving);
      delete root.dataset.hold;
      unstep?.();
      mutations.disconnect();
      removeEventListener("scroll", onScroll);
      removeEventListener("resize", onScroll);
      for (const s of sections()) delete s.dataset.focus;
    };
  }, [main, pathname]);
}
