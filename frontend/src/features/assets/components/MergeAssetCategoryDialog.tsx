import { useState } from "react";
import { plural } from "@/lib/format";
import { ConfirmDialog, SelectField } from "@/ui";
import type { AssetCategory } from "../api";
import { mergeTargets } from "../categoryView";
import { useMergeAssetCategory } from "../hooks";
import styles from "./MergeAssetCategoryDialog.module.css";

type Props = {
  category: AssetCategory;
  categories: AssetCategory[];
  onClose: () => void;
};

// Moves every asset of a category to another one, then deletes the first.
export function MergeAssetCategoryDialog({
  category,
  categories,
  onClose,
}: Props) {
  const merge = useMergeAssetCategory();
  const [target, setTarget] = useState("");
  const [failed, setFailed] = useState(false);
  const options = mergeTargets(categories, category);
  const n = category.assetCount;

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
      title={`Move ${plural(n, "asset")} from ${category.name}`}
      confirmLabel="Move and delete"
      cancelLabel="Cancel"
      confirmDisabled={!target}
      busy={merge.isPending}
      error={failed ? "We could not move the assets. Try again." : undefined}
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      <p>
        {n === 1 ? "The asset moves" : `All ${n} assets move`} to the category
        you pick. Then {category.name} is deleted.
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
