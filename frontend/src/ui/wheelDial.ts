import {
  atRest,
  calm,
  clamp,
  GLIDE,
  nearest,
  project,
  stepSpring,
  tick,
  WALK,
} from "./dayDial";
type Ref<T> = { current: T };
type Latest = { count: number; value: number; onPick: (index: number) => void };
type Drag = {
  y: number;
  pos: number;
  moved: boolean;
  samples: [time: number, y: number][];
};

// What a wheel (a column of values you roll through) remembers between events. It is the day strip's dial turned
// on its end and with a fixed list: the same spring, the same top speeds, the same tick. The column's position
// `pos` (in rows) is the one truth: it is written to a CSS variable and the stylesheet does the rest.
export type Wheel = {
  el: Ref<HTMLElement | null>;
  latest: Ref<Latest>;
  pos: number;
  vel: number;
  // The row the spring is carrying the column to, or null when it is still or being dragged.
  target: number | null;
  // Whether the page is told about the row when the column comes to rest (a drag, a flick, a wheel).
  tellAtRest: boolean;
  frame: number;
  last: number;
  lit: number;
  drag: Drag | null;
  blockClick: boolean;
  idle: number | undefined;
  turning: boolean;
  top: number;
  // The row the wheel last told the page about, so the page handing it back is not taken for a new request.
  told: number;
  typed: { text: string; at: number };
};

export const newWheel = (
  el: Ref<HTMLElement | null>,
  latest: Ref<Latest>,
): Wheel => ({
  el,
  latest,
  pos: 0,
  vel: 0,
  target: null,
  tellAtRest: false,
  frame: 0,
  last: 0,
  lit: -1,
  drag: null,
  blockClick: false,
  idle: undefined,
  turning: false,
  top: GLIDE,
  told: -1,
  typed: { text: "", at: 0 },
});

const count = (w: Wheel) => w.latest.current.count;

function write(w: Wheel, pos: number) {
  w.pos = pos;
  w.el.current?.style.setProperty("--pos", pos.toFixed(4));
  const lit = nearest(pos, count(w));
  if (lit !== w.lit) {
    if (w.lit >= 0) tick();
    w.lit = lit;
  }
}

function tell(w: Wheel, index: number) {
  if (index === w.latest.current.value) return;
  w.told = index;
  w.latest.current.onPick(index);
}

function still(w: Wheel) {
  cancelAnimationFrame(w.frame);
  w.frame = 0;
  w.target = null;
  w.vel = 0;
}

function run(w: Wheel, now: number) {
  const dt = Math.min(0.1, (now - w.last) / 1000);
  w.last = now;
  if (w.target === null) return;
  const next = stepSpring(w.pos, w.vel, w.target, dt, w.top);
  w.vel = next.vel;
  if (atRest(next.pos, next.vel, w.target)) {
    write(w, w.target);
    const settled = w.tellAtRest;
    still(w);
    if (settled) tell(w, nearest(w.pos, count(w)));
    return;
  }
  write(w, next.pos);
  w.frame = requestAnimationFrame((t) => run(w, t));
}

// A jump further than this rolls in from a few rows away instead of crossing every row between: a year picked
// from far off arrives in under a second, and each row is still seen going by.
const FAR = 12;
const ROLL_IN = 6;

// Carry the column to a row with the spring, from wherever it is and however fast it is going.
function aim(w: Wheel, row: number, tellAtRest: boolean) {
  const target = clamp(Math.round(row), 0, count(w) - 1);
  if (calm()) {
    still(w);
    write(w, target);
    if (tellAtRest) tell(w, target);
    return;
  }
  cancelAnimationFrame(w.frame);
  if (Math.abs(target - w.pos) > FAR)
    write(w, target - Math.sign(target - w.pos) * ROLL_IN);
  w.top = GLIDE;
  w.target = target;
  w.tellAtRest = tellAtRest;
  w.last = performance.now();
  w.frame = requestAnimationFrame((t) => run(w, t));
}

// Lets the spring chase a spot that need not be a whole row, for as long as a hand keeps moving it.
function chase(w: Wheel, spot: number) {
  const target = clamp(spot, 0, count(w) - 1);
  if (calm()) {
    still(w);
    write(w, target);
    tell(w, nearest(target, count(w)));
    return;
  }
  const running = w.frame !== 0;
  cancelAnimationFrame(w.frame);
  w.top = WALK;
  w.target = target;
  w.tellAtRest = true;
  if (!running) w.last = performance.now();
  w.frame = requestAnimationFrame((t) => run(w, t));
}

// The page says which row is picked. The first time the column is simply put there; after that it glides, unless
// the wheel itself just asked for this row or a hand is on it.
export function place(w: Wheel, index: number, mounted: boolean) {
  if (w.drag || index < 0) return;
  const echoed = index === w.told;
  w.told = -1;
  if (!mounted) {
    still(w);
    write(w, index);
  } else if (
    !echoed &&
    !w.turning &&
    w.target !== index &&
    Math.abs(w.pos - index) > 0.001
  )
    aim(w, index, false);
}

// A row asked for by a tap, a key or typing: the page is told at once and the column glides there.
export function choose(w: Wheel, row: number) {
  const index = clamp(Math.round(row), 0, count(w) - 1);
  tell(w, index);
  aim(w, index, false);
}

// One row up or down, and a held key keeps going.
export function step(w: Wheel, by: number) {
  choose(w, (w.target ?? Math.round(w.pos)) + by);
}

// A letter or digit typed on the wheel: the first row whose label starts with what was typed lately.
export function type(
  w: Wheel,
  char: string,
  labels: readonly string[],
  now = Date.now(),
) {
  const text = (
    now - w.typed.at > 600 ? char : w.typed.text + char
  ).toLowerCase();
  w.typed = { text, at: now };
  const index = labels.findIndex((label) =>
    label.toLowerCase().startsWith(text),
  );
  if (index >= 0) choose(w, index);
}

const MOVES: Record<string, number> = {
  ArrowUp: -1,
  ArrowDown: 1,
  PageUp: -5,
  PageDown: 5,
};

// A key pressed on the wheel. Returns whether the wheel used it.
export function key(
  w: Wheel,
  e: { key: string; ctrlKey: boolean; metaKey: boolean; altKey: boolean },
  labels: readonly string[],
): boolean {
  const move = MOVES[e.key];
  if (move) step(w, move);
  else if (e.key === "Home") choose(w, 0);
  else if (e.key === "End") choose(w, labels.length - 1);
  else if (
    e.key.length === 1 &&
    e.key !== " " &&
    !e.ctrlKey &&
    !e.metaKey &&
    !e.altKey
  )
    type(w, e.key, labels);
  else return false;
  return true;
}

const unit = (w: Wheel) =>
  w.el.current?.querySelector<HTMLElement>("[data-row]")?.offsetHeight || 40;

export function down(
  w: Wheel,
  e: { button: number; clientY: number; timeStamp: number },
) {
  if (e.button !== 0) return;
  still(w);
  w.drag = {
    y: e.clientY,
    pos: w.pos,
    moved: false,
    samples: [[e.timeStamp, e.clientY]],
  };
}

// The column follows the finger or the mouse one to one. Returns true once it counts as a drag and not a tap.
export function move(
  w: Wheel,
  e: { clientY: number; timeStamp: number },
): boolean {
  const g = w.drag;
  if (!g || (!g.moved && Math.abs(e.clientY - g.y) < 4))
    return Boolean(g?.moved);
  g.moved = true;
  g.samples = [...g.samples.slice(-5), [e.timeStamp, e.clientY]];
  write(w, clamp(g.pos - (e.clientY - g.y) / unit(w), -0.3, count(w) - 0.7));
  return true;
}

// Let go: the column coasts on at the speed it was dragged and lands on the row it would have reached.
export function up(w: Wheel) {
  const g = w.drag;
  w.drag = null;
  if (!g?.moved) return;
  w.blockClick = true;
  window.setTimeout(() => (w.blockClick = false), 0);
  const [from, to] = [g.samples[0], g.samples.at(-1)];
  const span = from && to ? to[0] - from[0] : 0;
  const velocity =
    from && to && span > 0 ? (-(to[1] - from[1]) / span / unit(w)) * 1000 : 0;
  aim(w, nearest(project(w.pos, velocity), count(w)), true);
}

// How far ahead of the column a hand may push it, in rows.
const LEAD = 3;

// A turn of the mouse wheel or the trackpad. It moves the spot the column is heading for, and the column runs
// after it at its usual pace, so every row passes one by one however hard the swipe. A notch moves one row.
export function wheel(w: Wheel, e: { deltaY: number }) {
  w.turning = true;
  const from = w.target ?? w.pos;
  const next =
    Math.abs(e.deltaY) >= 40
      ? Math.round(from) + Math.sign(e.deltaY)
      : from + e.deltaY / unit(w);
  chase(w, clamp(next, w.pos - LEAD, w.pos + LEAD));
  window.clearTimeout(w.idle);
  w.idle = window.setTimeout(() => {
    w.turning = false;
    aim(w, w.target ?? w.pos, true);
  }, 140);
}

export function dispose(w: Wheel) {
  window.clearTimeout(w.idle);
  still(w);
}
