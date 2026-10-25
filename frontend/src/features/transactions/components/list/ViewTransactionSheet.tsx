import { formatDate, formatMoney } from "@/lib/format";
import { FactList, RecordActions, Sheet } from "@/ui";
import type { Transaction } from "../../api/transactions";
import { typeLabel } from "../../model/labels";
import styles from "./ViewTransactionSheet.module.css";

type Props = {
  transaction: Transaction;
  onEdit: () => void;
  onDelete: () => void;
  onClose: () => void;
};

// A transaction as it was recorded: what it was for, the amount, and the facts under it. Changing or removing it is
// a deliberate next step, one tap on from here.
export function ViewTransactionSheet({
  transaction: t,
  onEdit,
  onDelete,
  onClose,
}: Props) {
  return (
    <Sheet
      open
      onClose={onClose}
      title="Transaction"
      footer={
        <RecordActions
          edit={{ label: "Edit", onClick: onEdit }}
          remove={[{ label: "Delete", onClick: onDelete }]}
        />
      }
    >
      <div className={styles.body}>
        <div className={styles.head}>
          <p className={styles.what}>{t.description || t.categoryName}</p>
          <p className={styles.amount}>{formatMoney(Math.abs(t.amount))}</p>
          <p className={styles.kind}>
            {typeLabel[t.transactionType]} · {t.categoryName}
          </p>
        </div>
        <FactList
          rows={[
            ["Type", typeLabel[t.transactionType]],
            ["Category", t.categoryName],
            ["Date", formatDate(t.transactionDate)],
            ["Note", t.description],
            ["Recorded on", formatDate(t.createdAt.slice(0, 10))],
          ]}
        />
      </div>
    </Sheet>
  );
}
