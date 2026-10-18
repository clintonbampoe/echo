import {
  categoryOptions,
  chooseType,
  countActive,
  financeFilters,
  periodDates,
  toApiFilters,
  typeOf,
} from "./filters";
import type { Category } from "./api";

const categories: Category[] = [
  { id: 1, name: "Offerings", categoryType: "Income", transactionCount: 38 },
  { id: 2, name: "Utilities", categoryType: "Expense", transactionCount: 14 },
];

describe("the period", () => {
  it("defaults to this week, as the server does", () => {
    expect(financeFilters.empty.window).toBe("week");
    expect(periodDates("week", "2026-10-14")).toEqual({
      from: "2026-10-11",
      to: "2026-10-14",
    });
  });

  it("covers this month, this year, last year and all time", () => {
    expect(periodDates("month", "2026-10-14")).toEqual({
      from: "2026-10-01",
      to: "2026-10-14",
    });
    expect(periodDates("year", "2026-10-14")).toEqual({
      from: "2026-01-01",
      to: "2026-10-14",
    });
    expect(periodDates("last-year", "2026-10-14")).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
    expect(periodDates("all", "2026-10-14").from).toBe("2000-01-01");
  });
});

describe("what the API is asked for", () => {
  it("always sends days, and the type and category only when chosen", () => {
    expect(toApiFilters(financeFilters.empty, "2026-10-14")).toEqual({
      transactionType: undefined,
      categoryId: undefined,
      from: "2026-10-11",
      to: "2026-10-14",
    });
    const picked = { type: "Income", category: "1", window: "year" };
    expect(toApiFilters(picked, "2026-10-14")).toMatchObject({
      transactionType: "Income",
      categoryId: 1,
    });
  });

  it("ignores a type or a category id that is not real", () => {
    expect(typeOf("Nonsense")).toBeUndefined();
    expect(
      toApiFilters({ type: "", category: "abc", window: "week" }, "2026-10-14")
        .categoryId,
    ).toBeUndefined();
  });
});

describe("the category list", () => {
  it("offers only the categories of the type on show", () => {
    expect(categoryOptions(categories, "Income").map((o) => o.label)).toEqual([
      "Category",
      "Offerings",
    ]);
    expect(categoryOptions(categories, "").map((o) => o.label)).toEqual([
      "Category",
      "Offerings",
      "Utilities",
    ]);
  });

  it("drops a chosen category that does not belong to the new type", () => {
    const chosen = { type: "", category: "1", window: "week" };
    expect(chooseType(chosen, "Expense", categories)).toEqual({
      type: "Expense",
      category: "",
    });
    expect(chooseType(chosen, "Income", categories)).toEqual({
      type: "Income",
      category: "1",
    });
    expect(chooseType(chosen, "", categories)).toEqual({
      type: "",
      category: "1",
    });
  });
});

describe("counting what is narrowing the list", () => {
  it("counts the category and a period that is not the default, never the type switch", () => {
    expect(countActive(financeFilters.empty)).toBe(0);
    expect(countActive({ type: "Income", category: "", window: "week" })).toBe(
      0,
    );
    expect(countActive({ type: "", category: "1", window: "year" })).toBe(2);
  });
});
