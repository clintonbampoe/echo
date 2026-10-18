import type { Category } from "./api";
import {
  categoriesOfType,
  categorySchema,
  mergeTargets,
  plural,
  toCategoryInput,
  usedBy,
} from "./categories";

const cats: Category[] = [
  { id: 1, name: "Offerings", categoryType: "Income", transactionCount: 38 },
  { id: 2, name: "Utilities", categoryType: "Expense", transactionCount: 14 },
  { id: 3, name: "Donations", categoryType: "Income", transactionCount: 0 },
];

describe("categories", () => {
  it("counts in words", () => {
    expect(usedBy(1)).toBe("1 transaction");
    expect(usedBy(0)).toBe("0 transactions");
    expect(plural(2, "category", "categories")).toBe("2 categories");
  });

  it("filters by type, or shows all", () => {
    expect(categoriesOfType(cats, "").length).toBe(3);
    expect(categoriesOfType(cats, "Income").map((c) => c.name)).toEqual([
      "Offerings",
      "Donations",
    ]);
    expect(categoriesOfType(undefined, "Income")).toEqual([]);
  });

  it("offers only other categories of the same type to move to", () => {
    expect(mergeTargets(cats, cats[0]!)).toEqual([
      { value: "3", label: "Donations" },
    ]);
    expect(mergeTargets(cats, cats[1]!)).toEqual([]);
  });

  it("needs a name and trims it", () => {
    expect(
      categorySchema.safeParse({ name: "  ", categoryType: "Income" }).success,
    ).toBe(false);
    expect(
      categorySchema.safeParse({
        name: "x".repeat(101),
        categoryType: "Income",
      }).success,
    ).toBe(false);
    expect(
      toCategoryInput({ name: " Tithes ", categoryType: "Income" }),
    ).toEqual({ name: "Tithes", categoryType: "Income" });
  });
});
