// What the tithe form accepts, and the words shown when it does not. The server has the final say; this
// catches the obvious mistakes first. No React in this file.
import { z } from "zod";
import { paymentOptions, type PaymentMethod } from "@/features/transactions";
import { monthNames } from "@/lib/dates";
import type { Option } from "@/ui";
import type { Tithe, TitheInput, TitheUpdate } from "./api";

export const NOTE_MAX = 2000;
export const AMOUNT_MAX = 1_000_000;

const methods = paymentOptions.map((o) => o.value);

// The first thing wrong with an amount as typed, or nothing. Commas are allowed ("1,250.50").
function amountProblem(text: string): string | null {
  if (text === "") return "Enter the amount";
  const plain = text.replace(/,/g, "");
  if (!/^\d+(\.\d{1,2})?$/.test(plain))
    return "Enter an amount like 150 or 150.50";
  if (Number(plain) <= 0) return "The amount must be more than zero";
  return Number(plain) > AMOUNT_MAX
    ? "The amount cannot be more than 1,000,000"
    : null;
}

// The form holds text for every field. `today` is read by the caller, so the tests can pass it.
export const titheSchema = (today: string) =>
  z.object({
    memberId: z.string().min(1, "Choose the member who gave"),
    amount: z
      .string()
      .trim()
      .superRefine((v, ctx) => {
        const message = amountProblem(v);
        if (message) ctx.addIssue({ code: "custom", message });
      }),
    collectionDate: z
      .string()
      .min(1, "Enter the day it was received")
      .refine(
        (v) => v <= today,
        "The day it was received cannot be in the future",
      ),
    forMonth: z
      .string()
      .refine((v) => monthNames.includes(v), "Choose a month"),
    forYear: z.string().regex(/^\d{4}$/, "Choose a year"),
    paymentMethod: z
      .string()
      .refine((v) => methods.includes(v), "Choose how it was paid"),
    description: z.string().max(NOTE_MAX),
  });

export type TitheValues = z.infer<ReturnType<typeof titheSchema>>;

// A new tithe starts on today, counting toward this month, paid in cash.
export function emptyTithe(today: string): TitheValues {
  return {
    memberId: "",
    amount: "",
    collectionDate: today,
    forMonth: monthNames[Number(today.slice(5, 7)) - 1] ?? "",
    forYear: today.slice(0, 4),
    paymentMethod: "Cash",
    description: "",
  };
}

export function fromTithe(t: Tithe): TitheValues {
  return {
    memberId: t.memberId,
    amount: String(t.amount),
    collectionDate: t.collectionDate.slice(0, 10),
    forMonth: t.forMonth,
    forYear: String(t.forYear),
    paymentMethod: t.paymentMethod,
    description: t.description ?? "",
  };
}

export function toTitheInput(v: TitheValues): TitheInput {
  return {
    memberId: v.memberId,
    amount: Number(v.amount.replace(/,/g, "")),
    forYear: Number(v.forYear),
    forMonth: v.forMonth,
    paymentMethod: v.paymentMethod as PaymentMethod,
    collectionDate: v.collectionDate,
    description: v.description.trim() || undefined,
  };
}

// Saving changes to a tithe: everything but the member, who cannot be changed.
export function toTitheUpdate(v: TitheValues): TitheUpdate {
  const {
    amount,
    forYear,
    forMonth,
    paymentMethod,
    collectionDate,
    description,
  } = toTitheInput(v);
  return {
    amount,
    forYear,
    forMonth,
    paymentMethod,
    collectionDate,
    description,
  };
}

// What stays for the next tithe in the same sitting: the day, the month it counts toward and how it was paid.
// The next tithe is usually someone else's, so the member, amount and note start empty.
export const nextTithe = (v: TitheValues): TitheValues => ({
  ...v,
  memberId: "",
  amount: "",
  description: "",
});

export const monthOptions: readonly Option[] = monthNames.map((m) => ({
  value: m,
  label: m,
}));

// Five years back to next year, plus the year of the tithe being edited when it is older.
export function yearOptions(
  today: string,
  include?: string,
): readonly Option[] {
  const now = Number(today.slice(0, 4));
  const years = new Set<number>(
    Array.from({ length: 7 }, (_, i) => now + 1 - i),
  );
  if (include) years.add(Number(include));
  return [...years]
    .sort((a, b) => b - a)
    .map((y) => ({ value: String(y), label: String(y) }));
}

// The month and year a day falls in, as the form holds them.
export const periodOf = (day: string) => ({
  forMonth: monthNames[Number(day.slice(5, 7)) - 1] ?? "",
  forYear: day.slice(0, 4),
});

// The API reports "memberId", "amount" and so on; the form uses the same names.
export const titheFieldMap = Object.fromEntries(
  [
    "memberId",
    "amount",
    "forYear",
    "forMonth",
    "paymentMethod",
    "collectionDate",
    "description",
  ].map((k) => [k, k]),
) as Record<keyof TitheValues, keyof TitheValues>;
