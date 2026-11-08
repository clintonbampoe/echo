import { focusOf, placeOf, REACH } from "./sectionFocus";

const WINDOW = 900;

describe("focusOf", () => {
  it("is 1 in place and 0 a reach away, on either side", () => {
    expect(focusOf(500, 500, 500, WINDOW)).toBe(1);
    expect(focusOf(500 + WINDOW * REACH, 500, 500, WINDOW)).toBe(0);
    expect(focusOf(500 - WINDOW * REACH, 500, 500, WINDOW)).toBe(0);
    expect(focusOf(5000, 500, 500, WINDOW)).toBe(0);
  });

  it("eases between, and never rises as the section gets further away", () => {
    let last = 1;
    for (let away = 0; away <= WINDOW * REACH; away += 30) {
      const f = focusOf(500 + away, 500, 500, WINDOW);
      expect(f).toBeLessThanOrEqual(last);
      last = f;
    }
    expect(focusOf(500 + (WINDOW * REACH) / 2, 500, 500, WINDOW)).toBeCloseTo(
      0.5,
    );
  });

  it("holds 1 for the whole stretch a tall section is in place", () => {
    expect(focusOf(700, 500, 900, WINDOW)).toBe(1);
    expect(focusOf(900, 500, 900, WINDOW)).toBe(1);
    expect(focusOf(900 + WINDOW * REACH, 500, 900, WINDOW)).toBe(0);
  });
});

describe("placeOf", () => {
  it("puts a short section in place when its top is at the window's top", () => {
    expect(placeOf(1200, 520, WINDOW, 3000)).toEqual({ from: 1200, to: 1200 });
  });

  it("gives the last sections the page's end, which they cannot scroll past", () => {
    expect(placeOf(3452, 386, WINDOW, 3178)).toEqual({ from: 3178, to: 3178 });
  });

  it("keeps a section taller than the window in place until it no longer covers the middle", () => {
    expect(placeOf(0, 1167, WINDOW, 507)).toEqual({ from: 0, to: 717 });
  });
});
