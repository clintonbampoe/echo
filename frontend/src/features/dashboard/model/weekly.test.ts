import { weekDates, weekFigures, weekMessage } from "./weekly";

const tithes = { total: 11300, tithers: 214, average: 53 };

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

  it("brings tithes, offerings and project gifts together, and keeps the biggest costs", () => {
    const c = (
      id: number,
      name: string,
      type: "Income" | "Expense",
      total: number,
    ) => ({
      categoryId: id,
      categoryName: name,
      type,
      total,
    });
    const f = weekFigures(
      [
        c(1, "Offering", "Income", 8000),
        c(2, "Seed", "Income", 400),
        c(3, "Utilities", "Expense", 1450),
        c(4, "Maintenance", "Expense", 980),
        c(5, "Salaries", "Expense", 500),
        c(6, "Fuel", "Expense", 300),
        c(7, "Tea", "Expense", 50),
      ],
      tithes,
      4600,
    );
    expect(f.came).toBe(24300);
    expect(f.cameRows.map((r) => r.amount)).toEqual([11300, 8400, 4600]);
    expect(f.spent).toBe(3280);
    expect(f.spentRows.map((r) => r.name)).toEqual([
      "Utilities",
      "Maintenance",
      "Salaries",
      "Other",
    ]);
    expect(f.net).toBe(21020);
    expect(weekMessage("this", f)).toBe(
      "This week: GHS 24.3k came in, 3.28k went out, net +21.02k. Thank you for giving.",
    );
  });

  it("shows a minus when more went out than came in", () => {
    const f = weekFigures(
      [{ categoryId: 1, categoryName: "Rent", type: "Expense", total: 900 }],
      { total: 0, tithers: 0, average: 0 },
      0,
    );
    expect(f.net).toBe(-900);
    expect(weekMessage("last", f)).toContain("net −900");
  });
});
