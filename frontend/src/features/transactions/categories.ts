// The Categories page: which categories show, how they are named, and what the form accepts. No React in this file.
import { z } from "zod";
import { plural } from "@/lib/format";
import { urlFilters } from "@/lib/urlFilters";
import type { Option } from "@/ui";
import type { Category, CategoryInput, TransactionType } from "./api";
import { transactionTypes } from "./filters";

export const NAME_MAX = 100;

// Income or expenses, one at a time; income first.
export const categoryFilters = urlFilters(
  { type: "Income" },
  { type: transactionTypes },
);

// The switch above the list: no "all", since a category is only ever one of the two.
export const categoryTypeOptions = [
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expenses" },
] as const;

// "6 categories · 37 transactions in use".
export const categoriesSummary = (categories: Category[]): string =>
  `${plural(categories.length, "category", "categories")} · ${plural(
    categories.reduce((sum, c) => sum + c.transactionCount, 0),
    "transaction",
  )} in use`;

export const usedBy = (n: number) => plural(n, "transaction");

// The categories of the type on show, or all of them. Income first, as the server sends them.
export const categoriesOfType = (
  categories: Category[] | undefined,
  type: string,
): Category[] =>
  (categories ?? []).filter((c) => !type || c.categoryType === type);

// "income" or "expense", for sentences.
export const typeWord = (type: TransactionType): string =>
  type === "Income" ? "income" : "expense";

// Where a category's transactions can move to: any other category of the same type (the server allows nothing else).
export const mergeTargets = (
  categories: Category[],
  source: Category,
): readonly Option[] =>
  categories
    .filter((c) => c.id !== source.id && c.categoryType === source.categoryType)
    .map((c) => ({ value: String(c.id), label: c.name }));

export const categorySchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Enter a name")
    .max(NAME_MAX, `Keep the name to ${NAME_MAX} characters or fewer`),
  categoryType: z.enum(["Income", "Expense"]),
});

export type CategoryValues = z.infer<typeof categorySchema>;

export const emptyCategory: CategoryValues = {
  name: "",
  categoryType: "Income",
};

export const toCategoryInput = (v: CategoryValues): CategoryInput => ({
  name: v.name.trim(),
  categoryType: v.categoryType,
});

// The API reports "name" and "categoryType"; the form uses the same names.
export const categoryFieldMap = {
  name: "name",
  categoryType: "categoryType",
} as const;
