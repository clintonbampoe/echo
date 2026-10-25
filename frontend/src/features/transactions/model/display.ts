// What the Finance page shows of its transactions: the numbers, the chart and the signed amount.
// Pure, so the tests can check it without a screen.
import { monthNames } from "@/lib/dates";
import { formatMoney, formatNumber } from "@/lib/format";
import type { DonutSlice, FlowBar, Stat } from "@/ui";
import type {
  Transaction,
  TransactionMonth,
  TransactionSummary,
  TransactionType,
} from "../api/transactions";

// Money in reads "+GH₵ 6,850", money out "−GH₵ 1,240" (a real minus sign, not a hyphen).
export const signedMoney = (amount: number, type: TransactionType): string =>
  `${type === "Income" ? "+" : "−"}${formatMoney(Math.abs(amount))}`;

export const transactionAmount = (
  t: Pick<Transaction, "amount" | "transactionType">,
) => signedMoney(t.amount, t.transactionType);

// The four numbers above the list: money in, money out, what is left, and how many transactions made them.
export function financeStats(summary: TransactionSummary | undefined): Stat[] {
  const s = summary;
  const count = s?.byCategory.reduce((sum, c) => sum + c.count, 0);
  return [
    {
      label: "Income",
      value: s ? formatNumber(s.totalIncome) : "–",
      unit: s ? "GHS" : undefined,
    },
    {
      label: "Expenses",
      value: s ? formatNumber(s.totalExpenses) : "–",
      unit: s ? "GHS" : undefined,
    },
    {
      label: "Net",
      value: s
        ? `${s.net < 0 ? "−" : "+"}${formatNumber(Math.abs(s.net))}`
        : "–",
      unit: s ? "GHS" : undefined,
    },
    {
      label: "Transactions",
      value: count === undefined ? "–" : formatNumber(count),
    },
  ];
}

// The biggest four categories as slices, the rest folded into "Other". Of the type on show, or of both.
export function categorySlices(
  summary: TransactionSummary | undefined,
  type: string,
): DonutSlice[] {
  const rows = (summary?.byCategory ?? [])
    .filter((c) => (!type || c.type === type) && c.total > 0)
    .sort((a, b) => b.total - a.total);
  const slice = (key: string, label: string, value: number): DonutSlice => ({
    key,
    label,
    value,
    text: formatMoney(value),
  });
  const top = rows
    .slice(0, 4)
    .map((c) => slice(String(c.categoryId), c.categoryName, c.total));
  const rest = rows.slice(4).reduce((sum, c) => sum + c.total, 0);
  return rest > 0 ? [...top, slice("other", "Other", rest)] : top;
}

// One month of the chart: both totals and what is left, for the figure above the bars.
export const flowBars = (rows: TransactionMonth[]): FlowBar[] =>
  rows.map((r) => {
    const short = (monthNames[r.month - 1] ?? "").slice(0, 3);
    return {
      key: `${r.year}-${String(r.month).padStart(2, "0")}`,
      label: short,
      name: `${monthNames[r.month - 1] ?? ""} ${r.year}`,
      income: r.totalIncome,
      expenses: r.totalExpenses,
    };
  });
