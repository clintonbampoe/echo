import { useLayoutEffect, type RefObject } from "react";
import { useLocation } from "react-router-dom";
import { useMedia } from "@/ui";
import { focusOf, placeOf } from "./sectionFocus";

// A section's content starts to arrive once it is this much in focus.
const ARRIVES = 0.4;
// Focus changes smaller than this are not worth a style change.
const STEP = 0.004;
// If the first look has not happened by now something is wrong; show everything rather than leave a blank page.
const FAILSAFE_MS = 1500;

type Place = {
  el: HTMLElement;
  from: number;
  to: number;
  shown: number;
  seen: boolean;
};

// Scrolling stays the browser's own, so it is as smooth as the device can make it. What moves with it is
// how much each section is in focus: --f runs from 1 (it is where it should be) down to 0 (a little over half
// a window away), and the section fades by that much (SiteLayout.module.css). The first time
// a section comes into focus its content arrives, one piece after another (data-seen). Nothing is held,
// gated or snapped. Off for reduced motion (live), and in browsers that cannot measure (tests).
export function useSectionFocus(main: RefObject<HTMLElement | null>) {
  const { pathname } = useLocation();
  const reduced = useMedia("(prefers-reduced-motion: reduce)", true);
  useLayoutEffect(() => {
    const el = main.current;
    if (
      !el ||
      reduced ||
      document.documentElement.dataset.reduceMotion === "true"
    )
      return;
    let places: Place[] = [];
    let frame = 0;
    let started = 0;
    let measured = false;

    // Where each section belongs on the page. Reads layout, so it runs only when the layout changes.
    function measure() {
      measured = true;
      const room = Math.max(
        0,
        document.documentElement.scrollHeight - innerHeight,
      );
      const before = new Map(places.map((p) => [p.el, p]));
      places = (Array.from(el!.children) as HTMLElement[]).map((s) => {
        const r = s.getBoundingClientRect();
        const old = before.get(s);
        return {
          el: s,
          ...placeOf(r.top + scrollY, r.height, innerHeight, room),
          shown: old?.shown ?? -1,
          seen: old?.seen ?? false,
        };
      });
      paint();
    }
    function paint() {
      frame = 0;
      for (const p of places) {
        const f = focusOf(scrollY, p.from, p.to, innerHeight);
        if (Math.abs(f - p.shown) < STEP && f !== 1 && f !== 0) continue;
        p.shown = f;
        p.el.style.setProperty("--f", f.toFixed(3));
        p.el.toggleAttribute("data-dim", f < 0.999);
        if (!p.seen && f >= ARRIVES) {
          p.seen = true;
          p.el.toggleAttribute("data-seen", true);
        }
      }
    }
    const onScroll = () => {
      frame ||= requestAnimationFrame(paint);
    };
    const first = () => {
      clearTimeout(started);
      started = window.setTimeout(measure, 60);
    };

    // Content waits, hidden, until its section is in focus. The first look is a moment after the
    // page is on screen so that the first section's content arrives too.
    el.toggleAttribute("data-motion", true);
    first();
    const failsafe = window.setTimeout(() => {
      if (measured) return;
      for (const s of Array.from(el.children))
        s.toggleAttribute("data-seen", true);
    }, FAILSAFE_MS);
    const resized =
      typeof ResizeObserver === "undefined"
        ? undefined
        : new ResizeObserver(measure);
    resized?.observe(el);
    // Pages load in as the route changes.
    const mutations = new MutationObserver(first);
    mutations.observe(el, { childList: true });
    addEventListener("scroll", onScroll, { passive: true });
    addEventListener("resize", measure);
    return () => {
      cancelAnimationFrame(frame);
      clearTimeout(started);
      clearTimeout(failsafe);
      resized?.disconnect();
      mutations.disconnect();
      removeEventListener("scroll", onScroll);
      removeEventListener("resize", measure);
      el.removeAttribute("data-motion");
      for (const s of Array.from(el.children)) {
        (s as HTMLElement).style.removeProperty("--f");
        s.removeAttribute("data-dim");
        s.removeAttribute("data-seen");
      }
    };
  }, [main, pathname, reduced]);
}
