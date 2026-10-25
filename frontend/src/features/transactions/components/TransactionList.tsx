import { formatDate, formatNumber } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { Chip, RecordTable } from "@/ui";
import type { Transaction } from "../api";
import { typeLabel, typeTone } from "../labels";
import { transactionAmount } from "../view";
import styles from "./TransactionList.module.css";
import type { TransactionStart } from "./TransactionDialogs";

const signedAmount = (t: Transaction) =>
  `${t.transactionType === "Income" ? "+" : "−"}${formatNumber(Math.abs(t.amount))}`;

type Props = {
  list: Paged<Transaction>;
  filtered: boolean;
  period: string;
  onOpen: (t: Transaction, start: TransactionStart) => void;
};

// Every transaction in the period, newest first. A table on a wide screen, a card for each on a phone. A row opens the
// transaction to read (rule S6); the ⋯ menu goes straight to editing or deleting.
export function TransactionList({ list, filtered, period, onOpen }: Props) {
  return (
    <RecordTable
      label="Transactions"
      title="Transactions"
      list={list}
      rowKey={(t) => t.id}
      large
      onOpen={(t) => onOpen(t, "view")}
      line={(t) => ({
        title: t.description || t.categoryName,
        sub: t.categoryName,
        end: (
          <strong
            className={t.transactionType === "Income" ? styles.in : styles.out}
          >
            {signedAmount(t)}
          </strong>
        ),
      })}
      rowName={(t) => t.description || t.categoryName}
      actions={(t) => [
        { label: "Edit transaction", onSelect: () => onOpen(t, "edit") },
        {
          label: "Delete transaction",
          danger: true,
          onSelect: () => onOpen(t, "delete"),
        },
      ]}
      noun={["transaction", "transactions"]}
      summary=". Newest first."
      empty={
        filtered
          ? {
              icon: "search",
              title: "No transactions match",
              text: "Try another category or type, or a wider period.",
            }
          : {
              icon: "finance",
              title: `No transactions ${period.toLowerCase()}`,
              text: "Record money in or money out, or choose a wider period.",
            }
      }
      columns={[
        { header: "Date", cell: (t) => formatDate(t.transactionDate) },
        { header: "Category", cell: (t) => <strong>{t.categoryName}</strong> },
        {
          header: "Description",
          cell: (t) => t.description || "No description",
        },
        {
          header: "Type",
          cell: (t) => (
            <Chip tone={typeTone[t.transactionType]}>
              {typeLabel[t.transactionType]}
            </Chip>
          ),
        },
        {
          header: "Amount",
          end: true,
          cell: (t) => (
            <strong
              className={
                t.transactionType === "Income" ? styles.in : styles.out
              }
            >
              {transactionAmount(t)}
            </strong>
          ),
        },
      ]}
    />
  );
}
