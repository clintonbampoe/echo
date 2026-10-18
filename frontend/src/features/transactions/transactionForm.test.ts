import type { Category, Transaction } from "./api";
import {
  asType,
  categoryChoices,
  clashesWith,
  emptyTransaction,
  fromTransaction,
  nextTransaction,
  toTransactionInput,
  toTransactionUpdate,
  transactionSchema,
  type TransactionValues,
} from "./transactionForm";

const categories: Category[] = [
  { id: 1, name: "Offerings", categoryType: "Income", transactionCount: 3 },
  { id: 2, name: "Utilities", categoryType: "Expense", transactionCount: 1 },
];
const filled: TransactionValues = {
  transactionType: "Expense",
  categoryId: "2",
  amount: "1,240.50",
  transactionDate: "2026-10-13",
  description: "  ECG bill  ",
};
const problems = (v: TransactionValues) => {
  const result = transactionSchema.safeParse(v);
  return result.success
    ? {}
    : Object.fromEntries(
        result.error.issues.map((i) => [String(i.path[0]), i.message]),
      );
};

describe("the transaction form", () => {
  it("starts as money in, dated today, with the category left to choose", () => {
    expect(emptyTransaction("2026-10-13")).toEqual({
      transactionType: "Income",
      categoryId: "",
      amount: "",
      transactionDate: "2026-10-13",
      description: "",
    });
  });

  it("accepts a filled form, commas in the amount and a note with spaces round it", () => {
    expect(problems(filled)).toEqual({});
    expect(toTransactionInput(filled)).toEqual({
      categoryId: 2,
      transactionType: "Expense",
      transactionDate: "2026-10-13",
      amount: 1240.5,
      description: "ECG bill",
    });
  });

  it("says what is missing, in the words of the design", () => {
    expect(
      problems({ ...emptyTransaction("2026-10-13"), transactionDate: "" }),
    ).toEqual({
      categoryId: "Choose a category",
      amount: "Enter the amount",
      transactionDate: "Enter the date",
    });
  });

  it("keeps the amount between 0.01 and 1,000,000", () => {
    for (const amount of ["0", "0.00", "1000000.01", "abc", "12.345"])
      expect(problems({ ...filled, amount }).amount).toBe(
        "Enter an amount between 0.01 and 1,000,000",
      );
    expect(problems({ ...filled, amount: "1000000" })).toEqual({});
    expect(problems({ ...filled, amount: "0.01" })).toEqual({});
  });

  it("leaves the note out of a new transaction when it is empty, but sends it empty to clear an old one", () => {
    const empty = { ...filled, description: "   " };
    expect(toTransactionInput(empty).description).toBeUndefined();
    expect(toTransactionUpdate(empty).description).toBe("");
  });

  it("shows a recorded transaction the way the form holds it", () => {
    const t: Transaction = {
      id: "t1",
      categoryId: 2,
      categoryName: "Utilities",
      transactionType: "Expense",
      transactionDate: "2026-10-13T00:00:00",
      amount: 1240,
      description: null,
      createdAt: "2026-10-13T09:00:00Z",
    };
    expect(fromTransaction(t)).toEqual({
      transactionType: "Expense",
      categoryId: "2",
      amount: "1,240.00",
      transactionDate: "2026-10-13",
      description: "",
    });
  });

  it("keeps the type and the day for the next one, and nothing else", () => {
    expect(nextTransaction(filled)).toEqual({
      ...filled,
      categoryId: "",
      amount: "",
      description: "",
    });
  });

  it("offers only the categories of the type chosen", () => {
    expect(categoryChoices(categories, "Income")).toEqual([
      { value: "1", label: "Offerings" },
    ]);
    expect(categoryChoices(undefined, "Income")).toEqual([]);
  });

  it("knows when the category chosen belongs to the other type", () => {
    expect(clashesWith(categories, "2", "Income")).toBe(true);
    expect(clashesWith(categories, "2", "Expense")).toBe(false);
    expect(clashesWith(categories, "", "Income")).toBe(false);
  });

  it("says what was recorded in plain words", () => {
    expect(asType("Income")).toBe("income");
    expect(asType("Expense")).toBe("an expense");
  });
});
