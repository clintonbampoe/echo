// What the contribution form accepts, and the words shown when it does not. The server has the final say; this
// catches the obvious mistakes first. No React in this file.
import { z } from "zod";
import { paymentOptions, type PaymentMethod } from "@/features/transactions";
import { amountProblem } from "@/lib/validation";
import type {
  Contribution,
  ContributionInput,
  ContributionUpdate,
} from "./api";

export const NOTE_MAX = 2000;
export const AMOUNT_MAX = 1_000_000;

const AMOUNT = {
  min: 0.01,
  max: AMOUNT_MAX,
  empty: "Enter the amount",
  range: "Enter an amount between 0.01 and 1,000,000",
};

const methods = paymentOptions.map((o) => o.value) as [
  PaymentMethod,
  ...PaymentMethod[],
];

// The form holds text for every field.
export const contributionSchema = z.object({
  amount: z
    .string()
    .trim()
    .superRefine((v, ctx) => {
      const message = amountProblem(v, AMOUNT);
      if (message) ctx.addIssue({ code: "custom", message });
    }),
  dateContributed: z.string().min(1, "Enter the date"),
  paymentMethod: z.enum(methods),
  description: z.string().max(NOTE_MAX, `Use ${NOTE_MAX} characters or fewer`),
});

export type ContributionValues = z.infer<typeof contributionSchema>;

// A new contribution starts as cash, dated today, with the amount to fill in.
export function emptyContribution(today: string): ContributionValues {
  return {
    amount: "",
    dateContributed: today,
    paymentMethod: "Cash",
    description: "",
  };
}

export function fromContribution(c: Contribution): ContributionValues {
  return {
    amount: c.amount.toLocaleString("en-US", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }),
    dateContributed: c.dateContributed.slice(0, 10),
    paymentMethod: c.paymentMethod,
    description: c.description ?? "",
  };
}

export function toContributionInput(
  projectId: string,
  v: ContributionValues,
): ContributionInput {
  return {
    projectId,
    amount: Number(v.amount.replace(/,/g, "")),
    dateContributed: v.dateContributed,
    paymentMethod: v.paymentMethod,
    description: v.description.trim() || null,
  };
}

// The create call must always carry the note, as null when there is none. Saving changes: an emptied note is sent as
// empty text, because leaving it out would keep the old one.
export function toContributionUpdate(
  v: ContributionValues,
): ContributionUpdate {
  const { projectId: _project, ...rest } = toContributionInput("", v);
  return { ...rest, description: v.description.trim() };
}

// What stays for the next gift in the same sitting: its day and method. The amount and note start empty.
export const nextContribution = (
  v: ContributionValues,
): ContributionValues => ({ ...v, amount: "", description: "" });

// The API reports "amount", "dateContributed" and so on; the form uses the same names.
export const contributionFieldMap = Object.fromEntries(
  ["amount", "dateContributed", "paymentMethod", "description"].map((k) => [
    k,
    k,
  ]),
) as Record<keyof ContributionValues, keyof ContributionValues>;
