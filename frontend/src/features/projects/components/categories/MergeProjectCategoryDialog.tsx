import { useState } from "react";
import { ConfirmDialog, SelectField } from "@/ui";
import type { ProjectCategory } from "../../api/categories";
import { plural } from "@/lib/format";
import { mergeTargets } from "../../model/categories";
import { useMergeProjectCategory } from "../../hooks/mutations";
import styles from "./MergeProjectCategoryDialog.module.css";

type Props = {
  category: ProjectCategory;
  categories: ProjectCategory[];
  onClose: () => void;
};

// Moves every project of a category to another one, then deletes the first.
export function MergeProjectCategoryDialog({
  category,
  categories,
  onClose,
}: Props) {
  const merge = useMergeProjectCategory();
  const [target, setTarget] = useState("");
  const [failed, setFailed] = useState(false);
  const options = mergeTargets(categories, category);
  const n = category.projectCount;

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
      title={`Move ${plural(n, "project")} from ${category.name}`}
      confirmLabel="Move and delete"
      tone="danger"
      cancelLabel="Cancel"
      confirmDisabled={!target}
      busy={merge.isPending}
      error={failed ? "We could not move the projects. Try again." : undefined}
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      <p>
        {n === 1 ? "The project moves" : `All ${n} projects move`} to the
        category you pick. Then {category.name} is deleted.
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
        <p>There is no other category to move them to. Create one first.</p>
      )}
    </ConfirmDialog>
  );
}
