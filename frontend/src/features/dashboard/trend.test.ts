import { bucketsOf, summarize, viewOf } from "./trend";

// A Monday: the week began on Sunday 18 October, so the last whole week is 11 to 17 October.
const TODAY = "2026-10-19";

describe("bucketsOf", () => {
  it("gives the last eight whole weeks, oldest first", () => {
    const weeks = bucketsOf(viewOf("weeks"), TODAY);
    expect(weeks).toHaveLength(8);
    expect(weeks[0]).toMatchObject({
      from: "2026-08-23",
      to: "2026-08-29",
      label: "23 Aug",
      name: "Week of 23 Aug",
      tip: "23 – 29 Aug",
    });
    expect(weeks[7]).toMatchObject({ from: "2026-10-11", to: "2026-10-17" });
  });

  it("gives whole months ending with last month", () => {
    const half = bucketsOf(viewOf("half"), TODAY);
    expect(half).toHaveLength(6);
    expect(half[0]).toMatchObject({
      from: "2026-04-01",
      to: "2026-04-30",
      label: "Apr",
      name: "April 2026",
      tip: "Apr 2026",
    });
    expect(half[5]).toMatchObject({ from: "2026-09-01", to: "2026-09-30" });
  });

  it("reaches back into the year before", () => {
    const year = bucketsOf(viewOf("year"), "2026-02-10");
    expect(year).toHaveLength(12);
    expect(year[0]).toMatchObject({ from: "2025-02-01", to: "2025-02-28" });
    expect(year[11]).toMatchObject({ from: "2026-01-01", to: "2026-01-31" });
  });
});

describe("summarize", () => {
  it("gives the latest, the average and the best", () => {
    expect(summarize([800, 930, 912])).toEqual({
      last: 912,
      average: 881,
      best: 930,
    });
  });

  it("has nothing to say about nothing", () => {
    expect(summarize([])).toBeUndefined();
  });
});
