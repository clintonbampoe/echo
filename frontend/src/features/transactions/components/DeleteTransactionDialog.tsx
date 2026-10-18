import { formatDate, formatMoney } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { Transaction } from "../api";
import { useDeleteTransaction } from "../hooks";
import { typeLabel } from "../labels";

type Props = { transaction: Transaction; onClose: () => void };

// "Delete" takes the transaction out of the lists and the totals. Most mistakes are better fixed by editing.
export function DeleteTransactionDialog({ transaction: t, onClose }: Props) {
  const remove = useDeleteTransaction();
  return (
    <DeleteDialog
      title="Delete this transaction?"
      noun="transaction"
      onDelete={() => remove.mutateAsync(t.id)}
      onClose={onClose}
    >
      <p>
        {formatMoney(t.amount)} {typeLabel[t.transactionType].toLowerCase()}{" "}
        under {t.categoryName}, dated {formatDate(t.transactionDate)}, will be
        removed from lists and totals.
      </p>
      <p>
        If the amount or category is wrong, edit the transaction instead. It
        keeps the record in place.
      </p>
    </DeleteDialog>
  );
}
