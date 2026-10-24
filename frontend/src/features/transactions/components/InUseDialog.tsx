import { ConfirmDialog, Notice } from "@/ui";
import type { Category } from "../api";
import { plural } from "@/lib/format";
import { typeWord } from "../categories";

type Props = { category: Category; onMove: () => void; onClose: () => void };

// A category with transactions cannot be deleted. Says so, and offers the way out: move them to another category.
export function InUseDialog({ category, onMove, onClose }: Props) {
  const n = category.transactionCount;
  return (
    <ConfirmDialog
      title={`${category.name} is still in use`}
      confirmLabel={`Move ${plural(n, "transaction")}`}
      cancelLabel="Cancel"
      tone="neutral"
      onConfirm={onMove}
      onCancel={onClose}
    >
      <p>
        {n === 1 ? "1 transaction is" : `${n} transactions are`} filed under{" "}
        {category.name}, so it cannot be deleted yet. Move{" "}
        {n === 1 ? "it" : "them"} to another {typeWord(category.categoryType)}{" "}
        category, then it can be deleted.
      </p>
      <Notice
        tone="warning"
        title="Moving keeps every transaction and its amount. Only the category changes."
      />
    </ConfirmDialog>
  );
}
