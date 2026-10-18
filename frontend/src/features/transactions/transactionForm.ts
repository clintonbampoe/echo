// What the transaction form accepts, and the words shown when it does not. The server has the final say; this
// catches the obvious mistakes first. No React in this file.
import { z } from "zod";
import type { Option } from "@/ui";
import type {
  Category,
  Transaction,
  TransactionInput,
  TransactionType,
  TransactionUpdate,
} from "./api";

export const NOTE_MAX = 2000;
export const AMOUNT_MAX = 1_000_000;

const AMOUNT_RANGE = "Enter an amount between 0.01 and 1,000,000";

// The first thing wrong with an amount as typed, or nothing. Commas are allowed ("1,250.50").
function amountProblem(text: string): string | null {
  if (text === "") return "Enter the amount";
  const plain = text.replace(/,/g, "");
  if (!/^\d+(\.\d{1,2})?$/.test(plain)) return AMOUNT_RANGE;
  const value = Number(plain);
  return value < 0.01 || value > AMOUNT_MAX ? AMOUNT_RANGE : null;
}

// The form holds text for every field.
export const transactionSchema = z.object({
  transactionType: z.enum(["Income", "Expense"]),
  categoryId: z.string().min(1, "Choose a category"),
  amount: z
    .string()
    .trim()
    .superRefine((v, ctx) => {
      const message = amountProblem(v);
      if (message) ctx.addIssue({ code: "custom", message });
    }),
  transactionDate: z.string().min(1, "Enter the date"),
  description: z.string().max(NOTE_MAX),
});

export type TransactionValues = z.infer<typeof transactionSchema>;

// A new transaction starts as money in, dated today, with the category left to choose.
export function emptyTransaction(today: string): TransactionValues {
  return {
    transactionType: "Income",
    categoryId: "",
    amount: "",
    transactionDate: today,
    description: "",
  };
}

export function fromTransaction(t: Transaction): TransactionValues {
  return {
    transactionType: t.transactionType,
    categoryId: String(t.categoryId),
    amount: t.amount.toLocaleString("en-US", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }),
    transactionDate: t.transactionDate.slice(0, 10),
    description: t.description ?? "",
  };
}

export function toTransactionInput(v: TransactionValues): TransactionInput {
  return {
    categoryId: Number(v.categoryId),
    transactionType: v.transactionType,
    transactionDate: v.transactionDate,
    amount: Number(v.amount.replace(/,/g, "")),
    description: v.description.trim() || undefined,
  };
}

// Saving changes: an emptied note is sent as empty text, because leaving it out would keep the old one.
export const toTransactionUpdate = (
  v: TransactionValues,
): TransactionUpdate => ({
  ...toTransactionInput(v),
  description: v.description.trim(),
});

// What stays for the next transaction in the same sitting: its type and day. The category, amount and note start empty.
export const nextTransaction = (v: TransactionValues): TransactionValues => ({
  ...v,
  categoryId: "",
  amount: "",
  description: "",
});

// Only the categories of the type chosen: an income category can hold no expense.
export const categoryChoices = (
  categories: Category[] | undefined,
  type: TransactionType,
): readonly Option[] =>
  (categories ?? [])
    .filter((c) => c.categoryType === type)
    .map((c) => ({ value: String(c.id), label: c.name }));

// Whether the category picked belongs to the other type, so changing the type has to clear it.
export const clashesWith = (
  categories: Category[] | undefined,
  categoryId: string,
  type: TransactionType,
): boolean =>
  Boolean(categoryId) &&
  (categories ?? []).some(
    (c) => String(c.id) === categoryId && c.categoryType !== type,
  );

// "as income" or "as an expense", for the line that says what was recorded.
export const asType = (type: TransactionType): string =>
  type === "Income" ? "income" : "an expense";

// The API reports "categoryId", "amount" and so on; the form uses the same names.
export const transactionFieldMap = Object.fromEntries(
  [
    "transactionType",
    "categoryId",
    "amount",
    "transactionDate",
    "description",
  ].map((k) => [k, k]),
) as Record<keyof TransactionValues, keyof TransactionValues>;
