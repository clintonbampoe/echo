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
type Latest = {
  days: string[];
  selected: string;
  onSelect: (day: string) => void;
};
type Drag = {
  x: number;
  pos: number;
  moved: boolean;
  samples: [time: number, x: number][];
};

// What the day strip's dial remembers between events. The row's position `pos` (in days) is the one truth: it is
// written to a CSS variable on the row, and the stylesheet turns it into where every day sits and how it looks.
export type Dial = {
  track: Ref<HTMLElement | null>;
  latest: Ref<Latest>;
  pos: number;
  vel: number;
  // The day a spring is carrying the row to, or null when it is still, being dragged or being scrolled by hand.
  target: number | null;
  frame: number;
  last: number;
  lit: number;
  drag: Drag | null;
  blockClick: boolean;
  idle: number | undefined;
  // True from the first sideways wheel event until the wheel has been quiet for a moment, so the tail of a trackpad swipe stays with the strip.
  turning: boolean;
  // The fastest the spring may run just now: a hand on the wheel keeps to a walk, everything else may glide.
  top: number;
  // A pending, not yet sent, word to the page about where the row is heading.
  telling: number | undefined;
  placed: string | null;
  // The day the row is showing in the slot, kept so a rebuilt row can tell which way a far jump travels.
  shown: string | null;
  // The day the dial last told the page about, so the page handing it back is not taken for a new request.
  told: number;
};

// The behaviour of the day strip lives here as plain functions on a `Dial`, with no React in it. The hook only
// wires them to a component's life.
export const newDial = (
  track: Ref<HTMLElement | null>,
  latest: Ref<Latest>,
): Dial => ({
  track,
  latest,
  pos: 0,
  vel: 0,
  target: null,
  frame: 0,
  last: 0,
  lit: -1,
  drag: null,
  blockClick: false,
  idle: undefined,
  turning: false,
  top: GLIDE,
  telling: undefined,
  placed: null,
  shown: null,
  told: -1,
});

const count = (d: Dial) => d.latest.current.days.length;

function write(d: Dial, pos: number) {
  d.pos = pos;
  d.track.current?.style.setProperty("--pos", pos.toFixed(4));
  const lit = nearest(pos, count(d));
  if (lit !== d.lit) {
    if (d.lit >= 0) tick();
    d.lit = lit;
    d.shown = d.latest.current.days[lit] ?? d.shown;
  }
}

// How long the row must keep to one heading before the page is told, so a long run does not make the page (the
// address bar, the server) work for every day that goes by. Once the row has stopped, the page is told at once.
const TELL_AFTER = 160;

// Tells the page which day the row is heading for (or has reached).
function tell(d: Dial, at: number, now = false) {
  window.clearTimeout(d.telling);
  const send = () => {
    const { days, selected, onSelect } = d.latest.current;
    const day = days[at];
    if (!day || day === selected) return;
    d.told = at;
    onSelect(day);
  };
  if (now) send();
  else d.telling = window.setTimeout(send, TELL_AFTER);
}

function still(d: Dial) {
  cancelAnimationFrame(d.frame);
  d.frame = 0;
  d.target = null;
  d.vel = 0;
}

function run(d: Dial, now: number) {
  const dt = Math.min(0.1, (now - d.last) / 1000);
  d.last = now;
  if (d.target === null) return;
  const next = stepSpring(d.pos, d.vel, d.target, dt, d.top);
  d.vel = next.vel;
  if (atRest(next.pos, next.vel, d.target)) {
    write(d, d.target);
    still(d);
    tell(d, nearest(d.pos, count(d)), true);
    return;
  }
  write(d, next.pos);
  d.frame = requestAnimationFrame((t) => run(d, t));
}

// Carry the row to a day with the spring, starting from wherever it is and however fast it is going.
function aim(d: Dial, day: number) {
  const target = clamp(Math.round(day), 0, count(d) - 1);
  if (calm()) {
    still(d);
    write(d, target);
    tell(d, target, true);
    return;
  }
  cancelAnimationFrame(d.frame);
  d.top = GLIDE;
  d.target = target;
  tell(d, target);
  d.last = performance.now();
  d.frame = requestAnimationFrame((t) => run(d, t));
}

// Lets the spring chase a spot that need not be a whole day, for as long as a hand keeps moving it. Like `aim`, but
// it does not round, so a trackpad swipe is followed at the same one-day-at-a-time pace as everything else.
function chase(d: Dial, spot: number) {
  const target = clamp(spot, 0, count(d) - 1);
  if (calm()) {
    still(d);
    write(d, target);
    tell(d, nearest(target, count(d)), true);
    return;
  }
  const running = d.frame !== 0;
  cancelAnimationFrame(d.frame);
  d.top = WALK;
  d.target = target;
  tell(d, nearest(target, count(d)));
  if (!running) d.last = performance.now();
  d.frame = requestAnimationFrame((t) => run(d, t));
}

const unit = (d: Dial) => {
  const [first, second] =
    d.track.current?.querySelectorAll<HTMLElement>("[data-day]") ?? [];
  return (first && second ? second.offsetLeft - first.offsetLeft : 0) || 80;
};

// A far jump rolls in from this many days away, in the direction it travels, so it arrives instead of appearing.
const ROLL_IN = 6;

// Slides the row over to where the day on show now sits, so nothing seems to move, and lets it carry on with what it was doing.
function slide(d: Dial, at: number, index: number) {
  const shift = at - d.lit;
  d.lit = at;
  write(d, d.pos + shift);
  if (d.target !== null) d.target += shift;
  else if (Math.abs(d.pos - index) > 0.001) aim(d, index);
}

// The row was rebuilt around a new centre. If the day on show is still on the new row, the row is slid over so
// nothing seems to move, and whatever it was doing carries on (a long run keeps running); otherwise it was a jump
// too far to glide over, so it rolls in from the side it came from. The first time, or with reduced motion, the day
// is simply put in the slot.
function arrive(d: Dial, index: number, mounted: boolean) {
  const from = d.shown;
  const to = d.latest.current.days[index];
  const at = from ? d.latest.current.days.indexOf(from) : -1;
  if (!mounted || calm() || !from || !to) {
    still(d);
    return write(d, index);
  }
  if (at >= 0) return slide(d, at, index);
  still(d);
  if (from === to) return write(d, index);
  write(d, clamp(index - (to > from ? 1 : -1) * ROLL_IN, 0, count(d) - 1));
  aim(d, index);
}

// The picked day is `index` on a list that starts at `first`. When the list is the same one, the row glides to the
// day (a tap, an arrow, the date box); when it was rebuilt, it arrives.
export function place(d: Dial, index: number, first: string | null) {
  if (d.drag || index < 0) return;
  const echoed = index === d.told;
  d.told = -1;
  const rebuilt = d.placed !== first;
  const mounted = d.placed !== null;
  d.placed = first;
  if (rebuilt) arrive(d, index, mounted);
  else if (
    !echoed &&
    !d.turning &&
    d.target !== index &&
    Math.abs(d.pos - index) > 0.001
  )
    aim(d, index);
}

export function down(
  d: Dial,
  e: { button: number; clientX: number; timeStamp: number },
) {
  if (e.button !== 0) return;
  still(d);
  d.drag = {
    x: e.clientX,
    pos: d.pos,
    moved: false,
    samples: [[e.timeStamp, e.clientX]],
  };
}

// The row follows the finger or the mouse one to one. Returns true once it counts as a drag and not a tap.
export function move(
  d: Dial,
  e: { clientX: number; timeStamp: number },
): boolean {
  const g = d.drag;
  if (!g || (!g.moved && Math.abs(e.clientX - g.x) < 4))
    return Boolean(g?.moved);
  g.moved = true;
  g.samples = [...g.samples.slice(-5), [e.timeStamp, e.clientX]];
  write(d, clamp(g.pos - (e.clientX - g.x) / unit(d), -0.3, count(d) - 0.7));
  return true;
}

// Let go: the row coasts on at the speed it was dragged and lands on the day it would have reached.
export function up(d: Dial) {
  const g = d.drag;
  d.drag = null;
  if (!g?.moved) {
    // A press that did not drag may have stopped a glide half way: finish it.
    if (g && Math.abs(d.pos - Math.round(d.pos)) > 0.001) aim(d, d.pos);
    return;
  }
  d.blockClick = true;
  window.setTimeout(() => (d.blockClick = false), 0);
  const [from, to] = [g.samples[0], g.samples.at(-1)];
  const span = from && to ? to[0] - from[0] : 0;
  const velocity =
    from && to && span > 0 ? (-(to[1] - from[1]) / span / unit(d)) * 1000 : 0;
  aim(d, nearest(project(d.pos, velocity), count(d)));
}

// How far ahead of the row a hand may push it, in days, so a hard swipe cannot pile up a long trip.
const LEAD = 3;

// A sideways turn of the wheel or trackpad. It moves the spot the row is heading for, and the row runs after it at
// its usual pace, so every day passes one by one however hard the swipe. A notch (a big jump, like shift and a mouse
// wheel) moves one day. Once a swipe has begun the strip keeps the rest of it, momentum included, so it runs out
// naturally; up and down on its own is left to the page, so the strip never steals the scroll from under the pointer.
export function wheel(d: Dial, e: { deltaX: number; deltaY: number }): boolean {
  if (!d.turning && (!e.deltaX || Math.abs(e.deltaX) <= Math.abs(e.deltaY)))
    return false;
  d.turning = true;
  if (e.deltaX) {
    const from = d.target ?? d.pos;
    const next =
      Math.abs(e.deltaX) >= 40
        ? Math.round(from) + Math.sign(e.deltaX)
        : from + e.deltaX / unit(d);
    chase(d, clamp(next, d.pos - LEAD, d.pos + LEAD));
  }
  window.clearTimeout(d.idle);
  d.idle = window.setTimeout(() => {
    d.turning = false;
    aim(d, d.target ?? d.pos);
  }, 140);
  return true;
}

export function step(d: Dial, by: number) {
  aim(d, (d.target ?? Math.round(d.pos)) + by);
}

export function dispose(d: Dial) {
  window.clearTimeout(d.idle);
  window.clearTimeout(d.telling);
  still(d);
}
