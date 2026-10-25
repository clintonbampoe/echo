import type { CategoryTotal } from "../api/money";
import { monthFigures, monthSlots, moneyRange, slicesOf } from "./money";

const TODAY = "2026-10-19";

const cat = (
  id: number,
  name: string,
  total: number,
  type: "Income" | "Expense" = "Income",
): CategoryTotal => ({ categoryId: id, categoryName: name, type, total });

describe("the months", () => {
  it("ends with this month, counted as far as today", () => {
    expect(moneyRange(TODAY, 6)).toEqual({ from: "2026-05-01", to: TODAY });
    expect(moneyRange(TODAY, 3)).toEqual({ from: "2026-08-01", to: TODAY });
    expect(moneyRange(TODAY, 12)).toEqual({ from: "2025-11-01", to: TODAY });
  });

  it("names each month before any money arrives", () => {
    const slots = monthSlots(TODAY, 6);
    expect(slots.map((s) => s.label)).toEqual([
      "May",
      "Jun",
      "Jul",
      "Aug",
      "Sep",
      "Oct",
    ]);
    expect(slots[5]).toMatchObject({ key: "2026-10", name: "October 2026" });
    expect(monthSlots(TODAY, 12)).toHaveLength(12);
  });

  it("matches the money to its month, and leaves it out until it arrives", () => {
    const slots = monthSlots(TODAY, 6);
    expect(monthFigures(slots, undefined)[0]).toEqual({ slot: slots[0] });
    const [may, , , , , oct] = monthFigures(slots, [
      { year: 2026, month: 5, totalIncome: 5500, totalExpenses: 3100 },
    ]);
    expect(may).toMatchObject({ income: 5500, expenses: 3100 });
    expect(oct).toMatchObject({ income: 0, expenses: 0 });
  });
});

describe("slicesOf", () => {
  it("lists up to five categories of one kind, biggest first", () => {
    const { total, slices } = slicesOf(
      [
        cat(1, "Donations", 12000),
        cat(2, "Offerings", 21600),
        cat(3, "Hall rental", 7200),
        cat(4, "Events", 4800),
        cat(5, "Misc", 2400),
        cat(6, "Salaries", 9000, "Expense"),
      ],
      "Income",
    );
    expect(total).toBe(48000);
    expect(slices.map((s) => [s.name, s.percent])).toEqual([
      ["Offerings", 45],
      ["Donations", 25],
      ["Hall rental", 15],
      ["Events", 10],
      ["Misc", 5],
    ]);
  });

  it("puts everything after the biggest four under Other", () => {
    const { slices } = slicesOf(
      [
        cat(1, "A", 500),
        cat(2, "B", 400),
        cat(3, "C", 300),
        cat(4, "D", 200),
        cat(5, "E", 100),
        cat(6, "F", 50),
      ],
      "Income",
    );
    expect(slices).toHaveLength(5);
    expect(slices[4]).toMatchObject({ name: "Other", total: 150 });
  });

  it("has no slices when nothing was recorded", () => {
    expect(slicesOf([], "Expense")).toEqual({ total: 0, slices: [] });
  });
});
