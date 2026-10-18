import { formatDate } from "@/lib/format";
import { Button, Chip, DetailGroup, Sheet } from "@/ui";
import type { Transaction } from "../api";
import { typeLabel, typeTone } from "../labels";
import { transactionAmount } from "../view";
import styles from "./ViewTransactionSheet.module.css";

type Props = {
  transaction: Transaction;
  onEdit: () => void;
  onDelete: () => void;
  onClose: () => void;
};

// A transaction as it was recorded, read only. Changing or removing it is a deliberate next step.
export function ViewTransactionSheet({
  transaction: t,
  onEdit,
  onDelete,
  onClose,
}: Props) {
  const footer = (
    <div className={styles.actions}>
      <Button variant="secondary" onClick={onDelete} className={styles.delete}>
        Delete transaction
      </Button>
      <Button onClick={onEdit}>Edit transaction</Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={onClose}
      title="Transaction"
      subtitle={t.categoryName}
      footer={footer}
    >
      <div className={styles.body}>
        <p
          className={`${styles.amount} ${t.transactionType === "Income" ? styles.in : ""}`}
        >
          {transactionAmount(t)}
        </p>
        <div>
          <Chip tone={typeTone[t.transactionType]}>
            {typeLabel[t.transactionType]}
          </Chip>
        </div>
        <DetailGroup
          title="Details"
          rows={[
            ["Category", t.categoryName],
            ["Date", formatDate(t.transactionDate)],
            ["Description", t.description],
            ["Recorded on", formatDate(t.createdAt.slice(0, 10))],
          ]}
        />
      </div>
    </Sheet>
  );
}
