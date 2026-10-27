import { monthNames } from "@/lib/dates";
import { formatNumber } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { Transaction } from "../../api/transactions";
import { useDeleteTransaction } from "../../hooks/mutations";

type Props = { transaction: Transaction; onClose: () => void };

const monthLabel = (ym: string) =>
  `${monthNames[Number(ym.slice(5, 7)) - 1] ?? ""}'s`;

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
        {t.description || t.categoryName} for GHS {formatNumber(t.amount)} will
        be removed from {monthLabel(t.transactionDate.slice(0, 7))} totals. This
        cannot be undone.
      </p>
    </DeleteDialog>
  );
}
