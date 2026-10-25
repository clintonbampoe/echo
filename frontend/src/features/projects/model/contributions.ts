// What a project's page shows of its money: the filters on its contributions, the numbers, and how far along it is.
// Pure, so the tests can check it without a screen. No React in this file.
import {
  paymentLabel,
  paymentOptions,
  type PaymentMethod,
} from "@/features/transactions";
import { ALL_TIME, monthStart } from "@/lib/dates";
import { formatMoney, formatNumber } from "@/lib/format";
import { urlFilters } from "@/lib/urlFilters";
import { withAny, type Option, type Stat } from "@/ui";
import type {
  ContributionFilters,
  ContributionSummary,
} from "../api/contributions";
import type { Project } from "../api/projects";

// The days the contributions cover. A project's own list starts at all time: what has been given, not what was given lately.
export type GivenKey = "all" | "month" | "year" | "last-year";

export const givenKeys: readonly GivenKey[] = [
  "all",
  "month",
  "year",
  "last-year",
];

export const givenOptions: readonly { value: GivenKey; label: string }[] = [
  { value: "all", label: "All time" },
  { value: "month", label: "This month" },
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
];

export const methodOptions: readonly Option[] = withAny(
  "Method",
  paymentOptions,
);

export const contributionFilters = urlFilters(
  { method: "", window: "all" },
  { method: ["", ...paymentOptions.map((o) => o.value)], window: givenKeys },
);

export type GivenFilters = typeof contributionFilters.empty;

export function givenDates(
  key: string,
  today: string,
): { from: string; to: string } {
  const year = Number(today.slice(0, 4));
  if (key === "month") return { from: monthStart(today), to: today };
  if (key === "year") return { from: `${year}-01-01`, to: today };
  if (key === "last-year")
    return { from: `${year - 1}-01-01`, to: `${year - 1}-12-31` };
  return ALL_TIME;
}

export const methodOf = (value: string): PaymentMethod | undefined =>
  paymentOptions.find((o) => o.value === value)?.value as
    PaymentMethod | undefined;

export const toContributionFilters = (
  projectId: string,
  f: GivenFilters,
  today: string,
): ContributionFilters => ({
  projectId,
  paymentMethod: methodOf(f.method),
  ...givenDates(f.window, today),
});

// What is left to raise, or by how much the target was beaten. The money stays honest past 100%.
export function remaining(p: Pick<Project, "raisedAmount" | "targetAmount">): {
  label: string;
  amount: number;
} {
  const left = p.targetAmount - p.raisedAmount;
  return left >= 0
    ? { label: "Still to raise", amount: left }
    : { label: "Over the target by", amount: -left };
}

// "Mobile money" for the name the server sends ("MobileMoney"); a name this app does not know is shown as sent.
export const methodName = (name: string | null | undefined): string =>
  name ? (paymentLabel[name as PaymentMethod] ?? name) : "–";

// The four numbers above the contributions. They follow the filters, so the notes say "in this view". The average
// is in whole cedis: it is a guide, not an account.
export function contributionStats(s: ContributionSummary | undefined): Stat[] {
  return [
    {
      label: "Contributed",
      value: s ? formatMoney(s.totalContributed) : "–",
      note: "In this view",
    },
    {
      label: "Contributions",
      value: s ? formatNumber(s.totalContributions) : "–",
      note: "In this view",
    },
    {
      label: "Average",
      value: s ? formatMoney(Math.round(s.averageAmount)) : "–",
      note: "Per contribution",
    },
    {
      label: "Most used method",
      value: s ? methodName(s.mostUsedPaymentMethod) : "–",
      note: "By number",
    },
  ];
}
