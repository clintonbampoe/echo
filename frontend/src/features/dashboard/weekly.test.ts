import { weekDates, weekFigures, weekMessage } from "./weekly";

describe("the weekly summary", () => {
  it("runs from Sunday to Saturday, and this week stops at today", () => {
    expect(weekDates("last", "2026-10-25")).toEqual({
      from: "2026-10-18",
      to: "2026-10-24",
    });
    expect(weekDates("this", "2026-10-28")).toEqual({
      from: "2026-10-25",
      to: "2026-10-28",
    });
  });

  it("adds income and expenses apart", () => {
    const f = weekFigures(
      [
        {
          categoryId: 1,
          categoryName: "Offering",
          type: "Income",
          total: 30000,
        },
        { categoryId: 2, categoryName: "Seed", type: "Income", total: 8200 },
        {
          categoryId: 3,
          categoryName: "Utilities",
          type: "Expense",
          total: 14300,
        },
      ],
      196,
    );
    expect(f).toEqual({ received: 38200, spent: 14300, present: 196 });
    expect(weekMessage("last", f)).toBe(
      "Last week we received GHS 38k and spent 14k. 196 people came. Thank you for giving.",
    );
  });

  it("leaves out who came when nobody was counted", () => {
    expect(weekMessage("this", { received: 0, spent: 0, present: 0 })).toBe(
      "This week we received GHS 0 and spent 0. Thank you for giving.",
    );
  });
});
