import { atRest, GLIDE, nearest, project, stepSpring, WALK } from "./dayDial";

describe("the dial", () => {
  it("runs a spring to its target without overshooting, and comes to rest there", () => {
    let state = { pos: 0, vel: 0 };
    let furthest = 0;
    for (let i = 0; i < 240; i++) {
      state = stepSpring(state.pos, state.vel, 5, 1 / 60);
      furthest = Math.max(furthest, state.pos);
    }
    expect(furthest).toBeLessThanOrEqual(5.01);
    expect(atRest(state.pos, state.vel, 5)).toBe(true);
  });

  it("takes a flick on at its own speed, and still lands on the target", () => {
    let state = { pos: 2, vel: 12 };
    for (let i = 0; i < 240; i++)
      state = stepSpring(state.pos, state.vel, project(2, 12), 1 / 60);
    expect(state.pos).toBeCloseTo(project(2, 12), 2);
  });

  it("projects a flick further the faster it goes, and keeps the day on the strip", () => {
    expect(project(10, 0)).toBe(10);
    expect(project(10, 5)).toBeGreaterThan(10);
    expect(project(10, -5)).toBeLessThan(10);
    expect(nearest(94.6, 91)).toBe(90);
    expect(nearest(-3, 91)).toBe(0);
    expect(nearest(4.4, 91)).toBe(4);
  });

  it("never runs faster than its top speed, so each day is seen going by", () => {
    for (const top of [WALK, GLIDE]) {
      let state = { pos: 0, vel: 0 };
      let fastest = 0;
      for (let i = 0; i < 240; i++) {
        state = stepSpring(state.pos, state.vel, 40, 1 / 60, top);
        fastest = Math.max(fastest, Math.abs(state.vel));
      }
      expect(fastest).toBeLessThanOrEqual(top);
    }
  });
});
