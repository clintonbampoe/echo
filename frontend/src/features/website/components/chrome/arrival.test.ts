import { describe, expect, it } from "vitest";
import { delaysOf } from "./arrival";

describe("delaysOf", () => {
  it("goes down the page row by row", () => {
    const d = delaysOf([
      { top: 300, left: 0 },
      { top: 0, left: 0 },
      { top: 120, left: 0 },
    ]);
    expect(d[1]).toBeLessThan(d[2] ?? 0);
    expect(d[2]).toBeLessThan(d[0] ?? 0);
  });

  it("goes left to right along a row", () => {
    const d = delaysOf([
      { top: 10, left: 400 },
      { top: 12, left: 0 },
    ]);
    expect(d[1]).toBeLessThan(d[0] ?? 0);
  });

  it("places a tall card by its middle so it follows the text beside it", () => {
    const d = delaysOf([
      { top: 0, left: 700, height: 240 },
      { top: 110, left: 0, height: 40 },
    ]);
    expect(d[1]).toBeLessThanOrEqual(d[0] ?? 0);
  });

  it("never makes anyone wait more than about half a second", () => {
    const boxes = Array.from({ length: 80 }, (_, i) => ({
      top: i * 100,
      left: 0,
    }));
    expect(Math.max(...delaysOf(boxes))).toBeLessThanOrEqual(520);
  });
});
