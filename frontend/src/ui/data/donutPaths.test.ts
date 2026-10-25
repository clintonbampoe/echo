import { donutPaths } from "./donutPaths";

describe("donut paths", () => {
  it("makes one slice for each value above zero, in order", () => {
    const shapes = donutPaths([
      { key: "a", value: 20 },
      { key: "b", value: 0 },
      { key: "c", value: 30 },
    ]);
    expect(shapes.map((s) => s.key)).toEqual(["a", "c"]);
    expect(shapes[0]?.path).toMatch(
      /^M[\d.]+ [\d.]+A64 64 0 0 1 .+A32 32 0 0 0 .+Z$/,
    );
  });

  it("marks a slice over half the ring as a large arc", () => {
    expect(
      donutPaths([
        { key: "a", value: 75 },
        { key: "b", value: 25 },
      ])[0]?.path,
    ).toContain("A64 64 0 1 1");
  });

  it("draws nothing when there is nothing to show", () => {
    expect(donutPaths([])).toEqual([]);
    expect(donutPaths([{ key: "a", value: 0 }])).toEqual([]);
  });
});
