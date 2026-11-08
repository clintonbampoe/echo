import { useLayoutEffect, type RefObject } from "react";
import { useLocation } from "react-router-dom";

// How far from its place a section can be (as a fraction of the window's height) before it has
// fully fallen back.
const REACH = 0.6;
// A section's content starts to arrive once it is this much in focus.
const ARRIVES = 0.4;

const smooth = (x: number) => x * x * (3 - 2 * x);

function wantsStillness() {
  return (
    typeof matchMedia !== "function" ||
    matchMedia("(prefers-reduced-motion: reduce)").matches ||
    document.documentElement.dataset.reduceMotion === "true"
  );
}

type Span = { el: HTMLElement; from: number; to: number; seen: boolean };

// Scrolling stays the browser's own, so it is as smooth as the device can make it. What moves with it is
// how much each section is in focus: --f runs from 1 (it is where it should be) down to 0 (a little over half
// a window away), and the section dims and blurs by that much (SiteLayout.module.css). The first time
// a section comes into focus its content arrives, one piece after another (data-seen). Nothing is held,
// gated or snapped. Off for reduced motion, and in browsers that cannot measure (tests).
export function useSectionFocus(main: RefObject<HTMLElement | null>) {
  const { pathname } = useLocation();
  useLayoutEffect(() => {
    const el = main.current;
    if (!el || wantsStillness()) return;
    let spans: Span[] = [];
    let frame = 0;
    let started = 0;

    // Where each section belongs on the page. A section taller than the window is in focus for as long
    // as it covers the window's middle; the last ones cannot reach the top, so the page's end is theirs.
    function measure() {
      const room = Math.max(
        0,
        document.documentElement.scrollHeight - innerHeight,
      );
      const known = new Map(spans.map((s) => [s.el, s.seen]));
      spans = (Array.from(el!.children) as HTMLElement[]).map((s) => {
        const r = s.getBoundingClientRect();
        const top = r.top + scrollY;
        const from = Math.min(top, room);
        const tall = r.height > innerHeight * 1.05;
        const to = tall
          ? Math.max(from, top + r.height - innerHeight / 2)
          : from;
        return { el: s, from, to, seen: known.get(s) ?? false };
      });
      paint();
    }
    function paint() {
      frame = 0;
      const y = scrollY;
      for (const s of spans) {
        const away = y < s.from ? s.from - y : Math.max(0, y - s.to);
        const f = 1 - smooth(Math.min(1, away / (innerHeight * REACH)));
        s.el.style.setProperty("--f", f.toFixed(3));
        s.el.toggleAttribute("data-dim", f < 0.999);
        if (!s.seen && f >= ARRIVES) {
          s.seen = true;
          s.el.dataset.seen = "";
        }
      }
    }
    const onScroll = () => {
      frame ||= requestAnimationFrame(paint);
    };

    // Content waits, hidden, until its section is in focus. The first look is a moment after the
    // page is on screen so that the first section's content arrives too.
    el.toggleAttribute("data-motion", true);
    started = window.setTimeout(measure, 60);
    const resized =
      typeof ResizeObserver === "undefined"
        ? undefined
        : new ResizeObserver(measure);
    resized?.observe(el);
    // Pages load in as the route changes.
    const mutations = new MutationObserver(() => {
      clearTimeout(started);
      started = window.setTimeout(measure, 60);
    });
    mutations.observe(el, { childList: true });
    addEventListener("scroll", onScroll, { passive: true });
    addEventListener("resize", measure);
    return () => {
      cancelAnimationFrame(frame);
      clearTimeout(started);
      resized?.disconnect();
      mutations.disconnect();
      removeEventListener("scroll", onScroll);
      removeEventListener("resize", measure);
      el.removeAttribute("data-motion");
      for (const s of spans) {
        s.el.style.removeProperty("--f");
        s.el.removeAttribute("data-dim");
        s.el.removeAttribute("data-seen");
      }
    };
  }, [main, pathname]);
}
