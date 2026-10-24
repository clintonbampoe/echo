import { tithesStat } from "./stat";

describe("tithesStat", () => {
  it("is the total this year and the months given", () => {
    const rows = [1, 2, 3].map((month) => ({
      month,
      totalCollected: month === 2 ? 0 : 100,
    }));
    expect(tithesStat(rows, "2026-03-20")).toMatchObject({
      value: "200",
      unit: "GHS",
      note: "2 of 3 months",
    });
  });
  it("is a dash until the months arrive", () => {
    expect(tithesStat(undefined, "2026-03-20").value).toBe("–");
  });
});
