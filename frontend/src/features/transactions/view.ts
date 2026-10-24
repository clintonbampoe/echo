// What the Finance page shows of its transactions: the numbers, the chart and the signed amount.
// Pure, so the tests can check it without a screen.
import { monthNames } from "@/lib/dates";
import { formatMoney, formatNumber } from "@/lib/format";
import type { MonthBar, Stat } from "@/ui";
import type {
  Transaction,
  TransactionMonth,
  TransactionSummary,
  TransactionType,
} from "./api";

// Money in reads "+GH₵ 6,850", money out "−GH₵ 1,240" (a real minus sign, not a hyphen).
export const signedMoney = (amount: number, type: TransactionType): string =>
  `${type === "Income" ? "+" : "−"}${formatMoney(Math.abs(amount))}`;

export const transactionAmount = (
  t: Pick<Transaction, "amount" | "transactionType">,
) => signedMoney(t.amount, t.transactionType);

// The four numbers above the list. `period` says which days they cover ("This week").
export function financeStats(
  summary: TransactionSummary | undefined,
  period: string,
): Stat[] {
  const s = summary;
  return [
    {
      label: "Income",
      value: s ? formatNumber(s.totalIncome) : "–",
      unit: s ? "GHS" : undefined,
      note: period,
    },
    {
      label: "Expenses",
      value: s ? formatNumber(s.totalExpenses) : "–",
      unit: s ? "GHS" : undefined,
      note: period,
    },
    {
      label: "Net",
      value: s
        ? `${s.net < 0 ? "−" : "+"}${formatNumber(Math.abs(s.net))}`
        : "–",
      unit: s ? "GHS" : undefined,
      note: "Income minus expenses",
    },
    {
      label: "Most active category",
      value: s?.mostActiveCategory || "–",
      note: "By number of transactions",
    },
  ];
}

// One bar per month, carrying both totals. The black bar is income, the grey one expenses.
export const chartBars = (rows: TransactionMonth[]): MonthBar[] =>
  rows.map((r) => {
    const short = (monthNames[r.month - 1] ?? "").slice(0, 3);
    return {
      key: `${r.year}-${String(r.month).padStart(2, "0")}`,
      label: short,
      name: `${monthNames[r.month - 1] ?? ""} ${r.year}`,
      tip: `${short} ${r.year}`,
      value: r.totalIncome,
      text: formatMoney(r.totalIncome),
      compare: { value: r.totalExpenses, text: formatMoney(r.totalExpenses) },
    };
  });
