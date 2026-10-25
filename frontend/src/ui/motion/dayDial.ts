// The numbers behind the day strip's dial. Positions are in days: 0 is the first day on the strip, 3.5 is halfway
// between the fourth and fifth. No React and no DOM in this file, except for the click.

// A spring a little past critical: it runs to its target without bouncing and without a pause at the end, which is
// what makes a swipe, a tap and a wheel notch feel like one motion.
const STIFFNESS = 205;
const DAMPING = 29;
const STEP = 1 / 240;
// The fastest the row may run, in days per second. Any quicker and days blur past; at this pace each one is seen going
// by, like a hand running along a row of bars. A hand on the trackpad or wheel keeps to WALK; a tap, an arrow or the
// end of a flick may glide a little quicker.
export const WALK = 9;
export const GLIDE = 12;

export const clamp = (n: number, low: number, high: number) =>
  Math.max(low, Math.min(high, n));

// Moves a spring `seconds` on toward `target`. `pos` and `vel` are in days and days per second, and `top` is the fastest it may go.
export function stepSpring(
  pos: number,
  vel: number,
  target: number,
  seconds: number,
  top = GLIDE,
): { pos: number; vel: number } {
  let p = pos;
  let v = vel;
  for (let left = seconds; left > 0; left -= STEP) {
    const dt = Math.min(STEP, left);
    v = clamp(v + (-STIFFNESS * (p - target) - DAMPING * v) * dt, -top, top);
    p += v * dt;
  }
  return { pos: p, vel: v };
}

// Close enough to stop: under a pixel from the target and no longer moving, so the long tail of the spring is not waited out.
export const atRest = (pos: number, vel: number, target: number) =>
  Math.abs(pos - target) < 0.01 && Math.abs(vel) < 0.15;

// Where a flick let go at `vel` days per second would coast to.
export const project = (pos: number, vel: number) =>
  pos + clamp(vel, -30, 30) * 0.18;

// The day nearest a position, kept on the strip.
export const nearest = (pos: number, count: number) =>
  clamp(Math.round(pos), 0, count - 1);

export const calm = () =>
  typeof window.matchMedia === "function" &&
  window.matchMedia("(prefers-reduced-motion: reduce)").matches;

// Each day that passes the middle gives a click, on phones that can buzz.
export function tick() {
  if (!calm()) navigator.vibrate?.(6);
}
