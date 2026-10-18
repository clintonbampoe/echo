import { useState } from "react";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Button, Icon, LinkButton, Notice, PageHeader, StatStrip } from "@/ui";
import type { Transaction } from "../api";
import { financeFilters, periodOptions, toApiFilters } from "../filters";
import {
  useCategories,
  useTransactions,
  useTransactionSummary,
} from "../hooks";
import { financeStats } from "../view";
import { FinanceChart } from "./FinanceChart";
import { FinanceFilterBar } from "./FinanceFilterBar";
import styles from "./FinancePage.module.css";
import { TransactionList } from "./TransactionList";
import { TransactionSheet } from "./TransactionSheet";
import { TransactionDialogs, type TransactionStart } from "./TransactionDialogs";

// The Finance page: what came in and what went out, the monthly chart, the filters, and every transaction.
// Tithes and project contributions are recorded on their own pages and are not counted here.
export function FinancePage() {
  const today = todayText();
  const [recording, setRecording] = useState(false);
  const [opened, setOpened] = useState<{
    transaction: Transaction;
    start: TransactionStart;
  } | null>(null);
  const { filters, update, clear } = useUrlFilters(financeFilters);
  const categories = useCategories();
  const query = toApiFilters(filters, today);
  const list = useTransactions(query);
  const summary = useTransactionSummary(query);
  const period =
    periodOptions.find((o) => o.value === filters.window)?.label ?? "";

  return (
    <div className="anim-rise">
      <PageHeader
        title="Finance"
        text="Money in and money out, apart from tithes."
        action={
          <>
            <LinkButton to={paths.financeCategories} variant="secondary">
              Categories
            </LinkButton>
            <Button onClick={() => setRecording(true)}>
              <Icon name="plus" size={16} />
              Record transaction
            </Button>
          </>
        }
      />
      <div className={styles.notice}>
        <Notice
          tone="warning"
          title="Tithes and project contributions are recorded on their own pages and are not part of these totals."
        />
      </div>
      <StatStrip
        label="Finance summary"
        stats={financeStats(summary.data, period)}
        busy={summary.isPending}
      />
      <FinanceChart today={today} />
      <FinanceFilterBar
        filters={filters}
        onChange={update}
        onClear={clear}
        categories={categories.data}
      />
      <TransactionList
        list={list}
        filtered={financeFilters.isFiltered(filters)}
        period={period}
        onOpen={(transaction, start) => setOpened({ transaction, start })}
      />
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
