import { growthBuckets, growthView } from "./growth";

const TODAY = "2026-10-19";

describe("growthBuckets", () => {
  it("covers whole months, and this one as far as today", () => {
    const buckets = growthBuckets(TODAY, 6);
    expect(buckets).toHaveLength(6);
    expect(buckets[0]).toMatchObject({
      from: "2026-05-01",
      to: "2026-05-31",
      label: "May",
    });
    expect(buckets[5]).toMatchObject({ from: "2026-10-01", to: TODAY });
  });
});

describe("growthView", () => {
  it("adds up the months and averages them", () => {
    expect(growthView([3, 5, 2, 6, 4, 9], 9)).toEqual({
      total: "29",
      average: "5",
      lastWeek: "9",
    });
  });

  it("has nothing to say until the numbers arrive", () => {
    expect(growthView(undefined, undefined)).toEqual({
      total: undefined,
      average: undefined,
      lastWeek: undefined,
    });
  });
});
