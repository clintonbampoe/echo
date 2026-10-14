import { useState, type ReactNode } from "react";
import { ConfirmDialog } from "./ConfirmDialog";

type Props = {
  // The question, e.g. "Delete Ama Boateng?"
  title: string;
  // What is being deleted, e.g. "member". The buttons and the error use it.
  noun: string;
  // Does the delete. It should throw when it fails.
  onDelete: () => Promise<unknown>;
  onClose: () => void;
  // Runs after a successful delete (a detail page uses it to go back to the list).
  onDeleted?: () => void;
  // What happens to the record and what is kept.
  children: ReactNode;
};

// "Delete" for any record: asks, runs the delete, closes, and says so when it fails.
export function DeleteDialog({
  title,
  noun,
  onDelete,
  onClose,
  onDeleted,
  children,
}: Props) {
  const [busy, setBusy] = useState(false);
  const [failed, setFailed] = useState(false);

  async function confirm() {
    setBusy(true);
    setFailed(false);
    try {
      await onDelete();
      onClose();
      onDeleted?.();
    } catch {
      setFailed(true);
    } finally {
      setBusy(false);
    }
  }

  return (
    <ConfirmDialog
      title={title}
      confirmLabel={`Delete ${noun}`}
      cancelLabel={`Keep ${noun}`}
      busy={busy}
      error={
        failed ? `We could not delete this ${noun}. Try again.` : undefined
      }
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      {children}
    </ConfirmDialog>
  );
}
