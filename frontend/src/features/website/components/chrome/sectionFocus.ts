// The arithmetic of section focus, apart from the page it runs on (useSectionFocus).

// How far from its place a section can be, as a fraction of the window's height, before it has
// fully fallen back.
export const REACH = 0.6;

const smooth = (x: number) => x * x * (3 - 2 * x);

// How much a section is in focus with the page scrolled to `y`: 1 anywhere from `from` to `to`, easing
// down to 0 a REACH of the window away on either side.
export function focusOf(
  y: number,
  from: number,
  to: number,
  windowHeight: number,
) {
  const away = y < from ? from - y : Math.max(0, y - to);
  return 1 - smooth(Math.min(1, away / (windowHeight * REACH)));
}

// The scroll positions at which a section is in place. A short one is in place when its top is at the
// window's top (or, for the last ones, when the page can scroll no further); a section taller than the
// window stays in place for as long as it covers the window's middle.
export function placeOf(
  top: number,
  height: number,
  windowHeight: number,
  room: number,
) {
  const from = Math.min(top, room);
  const tall = height > windowHeight * 1.05;
  return {
    from,
    to: tall ? Math.max(from, top + height - windowHeight / 2) : from,
  };
}
