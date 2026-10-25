import { useLayoutEffect, useRef } from "react";

// For a row of choices of different widths: a thumb that slides behind the picked one. Put the returned ref on
// the row, mark the picked child data-selected="true" (or let it carry aria-current="page"), and draw the thumb from the CSS variables --x, --w, --y and --h that
// this keeps up to date (it re-measures when the row or its choices change size). The thumb only starts
// animating after its first placement, so it does not slide in from the corner on load.
export function useSlidingThumb(selected: string) {
  const row = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    const el = row.current;
    if (!el) return;
    const place = () => {
      const on = el.querySelector<HTMLElement>(
        '[data-selected="true"], [aria-current="page"]',
      );
      // Nothing picked (a page that is not in the list): the thumb has nowhere to be, so it hides.
      el.dataset.has = on ? "true" : "false";
      el.style.setProperty("--x", `${on?.offsetLeft ?? 0}px`);
      el.style.setProperty("--w", `${on?.offsetWidth ?? 0}px`);
      // A column of choices (the sidebar) slides up and down by the same measure.
      el.style.setProperty("--y", `${on?.offsetTop ?? 0}px`);
      el.style.setProperty("--h", `${on?.offsetHeight ?? 0}px`);
    };
    place();
    const frame = requestAnimationFrame(() => {
      el.dataset.ready = "true";
    });
    const watcher =
      typeof ResizeObserver === "function"
        ? new ResizeObserver(place)
        : undefined;
    watcher?.observe(el);
    for (const child of el.children) watcher?.observe(child);
    return () => {
      cancelAnimationFrame(frame);
      watcher?.disconnect();
    };
  }, [selected]);

  return row;
}
