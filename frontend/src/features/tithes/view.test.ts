import { titheMonths, yearSlots } from "./view";

const rows = [1, 2, 3].map((month) => ({
  month,
  totalCollected: month === 2 ? 0 : 100,
}));

describe("titheMonths", () => {
  it("counts the months so far that had a tithe", () => {
    expect(titheMonths(rows, "2026-03-20")).toEqual({ given: 2, elapsed: 3 });
  });
  it("ignores months that have not come yet", () => {
    expect(
      titheMonths([{ month: 5, totalCollected: 100 }], "2026-03-20").given,
    ).toBe(0);
  });
});

describe("yearSlots", () => {
  it("gives twelve squares: given, not given, and the months still to come", () => {
    const slots = yearSlots(rows, "2026-03-20");
    expect(slots).toHaveLength(12);
    expect(slots.slice(0, 4).map((s) => s.state)).toEqual([
      "on",
      "off",
      "on",
      "future",
    ]);
    expect(slots[0]?.name).toBe("January");
  });
});
