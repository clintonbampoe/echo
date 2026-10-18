import { useState } from "react";
import type { Transaction } from "../api";
import { DeleteTransactionDialog } from "./DeleteTransactionDialog";
import { EditTransactionSheet } from "./EditTransactionSheet";
import { ViewTransactionSheet } from "./ViewTransactionSheet";

// Where a transaction can be opened at: read only, to change, or to delete.
export type TransactionStart = "view" | "edit" | "delete";

type Props = {
  transaction: Transaction;
  start: TransactionStart;
  onClose: () => void;
};

// Everything that can happen to one existing transaction, one step at a time: a transaction opens to be read, and
// from there it can be edited or deleted.
export function TransactionDialogs({ transaction, start, onClose }: Props) {
  const [step, setStep] = useState<TransactionStart>(start);
  if (step === "view")
    return (
      <ViewTransactionSheet
        transaction={transaction}
        onEdit={() => setStep("edit")}
        onDelete={() => setStep("delete")}
        onClose={onClose}
      />
    );
  if (step === "delete")
    return (
      <DeleteTransactionDialog transaction={transaction} onClose={onClose} />
    );
  return <EditTransactionSheet transaction={transaction} onClose={onClose} />;
}
