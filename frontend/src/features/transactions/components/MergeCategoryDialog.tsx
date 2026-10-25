import { useState } from "react";
import { ConfirmDialog, SelectField } from "@/ui";
import type { Category } from "../api";
import { plural } from "@/lib/format";
import { mergeTargets, typeWord } from "../categories";
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
      title={`${category.name} is still in use`}
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
        {plural(n, "transaction")} {n === 1 ? "uses" : "use"} this category.
        Move {n === 1 ? "it" : "them"} to another {word} category, then it can
        be deleted.
      </p>
      {options.length > 0 ? (
        <div className={styles.pick}>
          <SelectField
            label="Move to"
            placeholder="Choose a category"
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
