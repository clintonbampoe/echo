// A mouse wheel, a trackpad or a key moves the page one section at a time (see useSectionFocus). The
// page glides to the next section's top, and nothing else moves it until it has arrived.

const QUIET_MS = 120; // wheel events closer than this belong to one gesture
const EDGE = 8; // pixels of slack when asking if a section is lined up
const GLIDE_MS = 700;
// Space on these means "press", not "scroll".
const PRESSABLE = "button, a, summary, [role='tab'], [role='switch']";
const TYPING = "input, textarea, select, [contenteditable='true']";

type Gate = {
  // True while the page is moving or holding for an entrance.
  busy: () => boolean;
  // Called as a glide begins.
  start: () => void;
};

const ease = (t: number) => (t < 0.5 ? 4 * t ** 3 : 1 - (-2 * t + 2) ** 3 / 2);

// Something inside the page (the steps window) that can still scroll the way the wheel is going.
function scrollsInside(
  target: EventTarget | null,
  main: HTMLElement,
  dy: number,
) {
  let node = target instanceof Element ? target : null;
  for (; node && node !== main; node = node.parentElement) {
    if (!/auto|scroll/.test(getComputedStyle(node).overflowY)) continue;
    const room =
      dy > 0
        ? node.scrollHeight - node.clientHeight - node.scrollTop
        : node.scrollTop;
    if (node.scrollHeight > node.clientHeight && room > 1) return true;
  }
  return false;
}

// Where one step in this direction lands. undefined leaves the scroll to the browser: a section taller
// than the window is read by scrolling it, and past the first or last section there is nothing to step to.
function stepTarget(sections: HTMLElement[], dir: 1 | -1) {
  const rects = sections.map((s) => s.getBoundingClientRect());
  const i = rects.findIndex((r) => r.bottom > EDGE);
  const r = rects[i];
  if (!r) return undefined;
  const tall = r.height > innerHeight * 1.05;
  if (dir > 0)
    return tall && r.bottom > innerHeight + EDGE ? undefined : sections[i + 1];
  if (r.top < -EDGE) return tall ? undefined : sections[i];
  return sections[i - 1];
}

const KEYS: Record<string, 1 | -1> = {
  PageDown: 1,
  ArrowDown: 1,
  PageUp: -1,
  ArrowUp: -1,
};

// Keys typed into a field, or pressed on a button or link, are not ours.
function ignored(e: KeyboardEvent) {
  const el = e.target instanceof Element ? e.target : null;
  const pressing = e.key === " " ? `${TYPING}, ${PRESSABLE}` : TYPING;
  return (
    e.defaultPrevented ||
    e.altKey ||
    e.ctrlKey ||
    e.metaKey ||
    !!el?.closest(pressing)
  );
}

// What a key means here: 1 down, -1 up, 0 not ours.
function directionOf(e: KeyboardEvent): 1 | -1 | 0 {
  if (ignored(e)) return 0;
  if (e.key === " ") return e.shiftKey ? -1 : 1;
  return KEYS[e.key] ?? 0;
}

export function stepSections(main: HTMLElement, gate: Gate) {
  let frame = 0;
  let lastWheel = 0;
  let used = false;

  const glide = (to: number) => {
    const from = scrollY;
    const begun = performance.now();
    const tick = (now: number) => {
      const t = Math.min(1, (now - begun) / GLIDE_MS);
      scrollTo(0, from + (to - from) * ease(t));
      frame = t < 1 ? requestAnimationFrame(tick) : 0;
    };
    frame = requestAnimationFrame(tick);
  };
  // Returns false when the browser should scroll as usual.
  const step = (dir: 1 | -1) => {
    const target = stepTarget(Array.from(main.children) as HTMLElement[], dir);
    if (!target) return false;
    if (gate.busy() || used) return true;
    used = true;
    gate.start();
    glide(scrollY + target.getBoundingClientRect().top);
    return true;
  };

  const onWheel = (e: WheelEvent) => {
    if (e.ctrlKey || !e.deltaY || Math.abs(e.deltaX) > Math.abs(e.deltaY))
      return;
    const now = performance.now();
    if (now - lastWheel > QUIET_MS) used = false;
    lastWheel = now;
    if (scrollsInside(e.target, main, e.deltaY)) {
      used = true;
      return;
    }
    if (step(e.deltaY > 0 ? 1 : -1)) e.preventDefault();
  };
  const onKey = (e: KeyboardEvent) => {
    const dir = directionOf(e);
    used = false;
    if (dir && step(dir)) e.preventDefault();
  };
  // Someone grabbing the scrollbar or touching the page takes over from the glide.
  const onGrab = () => {
    cancelAnimationFrame(frame);
    frame = 0;
  };

  addEventListener("wheel", onWheel, { passive: false });
  addEventListener("keydown", onKey);
  addEventListener("pointerdown", onGrab);
  return () => {
    cancelAnimationFrame(frame);
    removeEventListener("wheel", onWheel);
    removeEventListener("keydown", onKey);
    removeEventListener("pointerdown", onGrab);
  };
}
