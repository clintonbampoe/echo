import { useState } from "react";
import { ConfirmDialog, SelectField } from "@/ui";
import type { Category } from "../api";
import { mergeTargets, plural, typeWord } from "../categories";
import { useMergeCategory } from "../hooks";
import styles from "./MergeCategoryDialog.module.css";

type Props = {
  category: Category;
  categories: Category[];
  onClose: () => void;
};

// Moves every transaction of a category to another of the same type, then deletes the first.
export function MergeCategoryDialog({ category, categories, onClose }: Props) {
  const merge = useMergeCategory();
  const [target, setTarget] = useState("");
  const [failed, setFailed] = useState(false);
  const options = mergeTargets(categories, category);
  const n = category.transactionCount;
  const word = typeWord(category.categoryType);

  async function confirm() {
    setFailed(false);
    try {
      await merge.mutateAsync({
        id: category.id,
        targetCategoryId: Number(target),
      });
      onClose();
    } catch {
      setFailed(true);
    }
  }

  return (
    <ConfirmDialog
      title={`Move ${plural(n, "transaction")} from ${category.name}`}
      confirmLabel="Move and delete"
      cancelLabel="Cancel"
      confirmDisabled={!target}
      busy={merge.isPending}
      error={
        failed ? "We could not move the transactions. Try again." : undefined
      }
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      <p>
        {n === 1 ? "The transaction moves" : `All ${n} transactions move`} to
        the category you pick. Then {category.name} is deleted.
      </p>
      {options.length > 0 ? (
        <div className={styles.pick}>
          <SelectField
            label="Move to"
            placeholder={`Choose ${word === "income" ? "an income" : "an expense"} category`}
            options={options}
            value={target}
            onChange={setTarget}
          />
        </div>
      ) : (
        <p>
          There is no other {word} category to move them to. Create one first.
        </p>
      )}
    </ConfirmDialog>
  );
}
