import { useState } from "react";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import {
  Aside,
  DashCard,
  Donut,
  LinkButton,
  PageHeader,
  StatStrip,
} from "@/ui";
import type { Transaction } from "../api";
import {
  financeFilters,
  monthLabel,
  periodOptions,
  toApiFilters,
} from "../filters";
import {
  useCategories,
  useTransactions,
  useTransactionSummary,
} from "../hooks";
import { categorySlices, financeStats } from "../view";
import { FinanceChart } from "./FinanceChart";
import { FinanceFilterBar } from "./FinanceFilterBar";
import styles from "./FinancePage.module.css";
import {
  TransactionDialogs,
  type TransactionStart,
} from "./TransactionDialogs";
import { TransactionList } from "./TransactionList";
import { TransactionSheet } from "./TransactionSheet";
import { useUrlFlag } from "@/lib/useUrlFlag";

// The Finance page: what came in and what went out, the monthly chart, the filters, and every transaction.
// Tithes and project contributions are recorded on their own pages and are not counted here.
export function FinancePage() {
  const today = todayText();
  const [recording, setRecording] = useUrlFlag("new");
  const [opened, setOpened] = useState<{
    transaction: Transaction;
    start: TransactionStart;
  } | null>(null);
  const { filters, update, clear } = useUrlFilters(financeFilters);
  const categories = useCategories();
  const query = toApiFilters(filters, today);
  const list = useTransactions(query);
  const summary = useTransactionSummary(query);
  const period = filters.month
    ? monthLabel(filters.month)
    : (periodOptions.find((o) => o.value === filters.window)?.label ?? "");

  return (
    <div className="anim-rise">
      <PageHeader
        title="Finance"
        action={
          <LinkButton to={paths.financeCategories} variant="secondary">
            Categories
          </LinkButton>
        }
        add={{ label: "New transaction", onClick: () => setRecording(true) }}
      />
      <StatStrip
        label="Finance summary"
        stats={financeStats(summary.data)}
        busy={summary.isPending}
      />
      <FinanceChart
        today={today}
        month={filters.month}
        onPick={(month) => update({ month })}
      />
      <FinanceFilterBar
        filters={filters}
        onChange={update}
        onClear={clear}
        categories={categories.data}
        today={today}
      />
      <div className={styles.split}>
        <TransactionList
          list={list}
          filtered={financeFilters.isFiltered(filters)}
          period={period}
          onOpen={(transaction, start) => setOpened({ transaction, start })}
        />
        <DashCard title="By category" control={<Aside>{period}</Aside>}>
          <Donut
            label="Transactions by category"
            slices={categorySlices(summary.data, filters.type)}
            stacked
          />
        </DashCard>
      </div>
      <p className={styles.note}>
        Tithes and project contributions are recorded on their own pages and are
        not part of these totals.
      </p>
      <TransactionSheet
        open={recording}
        today={today}
        onClose={() => setRecording(false)}
      />
      {opened && (
        <TransactionDialogs
          transaction={opened.transaction}
          start={opened.start}
          onClose={() => setOpened(null)}
        />
      )}
    </div>
  );
}
