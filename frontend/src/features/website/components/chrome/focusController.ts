import { arrive, delaysOf, piecesOf } from "./arrival";
import { focusOf, placeOf } from "./sectionFocus";

// A section's content starts to arrive once it is this much in focus.
const ARRIVES = 0.4;
// Focus changes smaller than this are not worth a style change.
const STEP = 0.004;
// The first look is a moment after the page is on screen, so that the first section's content arrives too.
const FIRST_LOOK_MS = 60;
// If the first look has not happened by now something is wrong; show everything rather than leave a blank page.
const FAILSAFE_MS = 1500;

type Place = {
  el: HTMLElement;
  from: number;
  to: number;
  shown: number;
  seen: boolean;
};
type Arrival = { pieces: HTMLElement[]; delays: number[] };

const sectionsOf = (main: HTMLElement) =>
  Array.from(main.children) as HTMLElement[];

// Gives every section of `main` focus as the page scrolls and has its content arrive the first time it is in
// focus (see useSectionFocus). Returns what undoes it all.
export function startFocus(main: HTMLElement) {
  let places: Place[] = [];
  const arrivals = new Map<Element, Arrival>();
  let frame = 0;
  let look = 0;
  let measured = false;

  // What arrives in each section, and when. Before anything is painted, so nothing flashes. A section whose
  // content is still being put together is looked at again until it has had its turn.
  function arm() {
    for (const s of sectionsOf(main)) {
      if (places.some((p) => p.el === s && p.seen)) continue;
      const old = arrivals.get(s);
      const pieces = piecesOf(s);
      const delays = delaysOf(pieces.map((p) => p.getBoundingClientRect()));
      for (const p of old?.pieces ?? []) p.removeAttribute("data-arrive");
      for (const p of pieces) p.toggleAttribute("data-arrive", true);
      arrivals.set(s, { pieces, delays });
    }
  }
  // Where each section belongs on the page. Reads layout, so it runs only when the layout changes.
  function measure() {
    measured = true;
    // Styles may have landed since the first look; what is a card is read again, in the same task as the paint.
    arm();
    const room = Math.max(
      0,
      document.documentElement.scrollHeight - innerHeight,
    );
    const before = new Map(places.map((p) => [p.el, p]));
    places = sectionsOf(main).map((el) => {
      const r = el.getBoundingClientRect();
      const old = before.get(el);
      return {
        el,
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
      if (p.seen || f < ARRIVES) continue;
      p.seen = true;
      p.el.toggleAttribute("data-seen", true);
      const a = arrivals.get(p.el);
      if (a) arrive(a.pieces, a.delays);
    }
  }
  const onScroll = () => {
    frame ||= requestAnimationFrame(paint);
  };
  const soon = () => {
    clearTimeout(look);
    look = window.setTimeout(measure, FIRST_LOOK_MS);
  };

  main.toggleAttribute("data-motion", true);
  arm();
  soon();
  const failsafe = window.setTimeout(() => {
    if (!measured)
      for (const s of sectionsOf(main)) s.toggleAttribute("data-seen", true);
  }, FAILSAFE_MS);
  const resized =
    typeof ResizeObserver === "undefined"
      ? undefined
      : new ResizeObserver(measure);
  resized?.observe(main);
  // Pages load in as the route changes, and parts of them a little after.
  const mutations = new MutationObserver(() => {
    arm();
    soon();
  });
  mutations.observe(main, { childList: true, subtree: true });
  addEventListener("scroll", onScroll, { passive: true });
  addEventListener("resize", measure);

  return () => {
    cancelAnimationFrame(frame);
    clearTimeout(look);
    clearTimeout(failsafe);
    resized?.disconnect();
    mutations.disconnect();
    removeEventListener("scroll", onScroll);
    removeEventListener("resize", measure);
    main.removeAttribute("data-motion");
    for (const a of arrivals.values()) {
      for (const p of a.pieces) p.removeAttribute("data-arrive");
    }
    for (const s of sectionsOf(main)) {
      s.style.removeProperty("--f");
      s.removeAttribute("data-dim");
      s.removeAttribute("data-seen");
    }
  };
}
