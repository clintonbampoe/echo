import { useState } from "react";
import type { AssetCategory } from "../api";
import { AssetCategoryInUseDialog } from "./AssetCategoryInUseDialog";
import type { CategoryStart } from "./AssetCategoryList";
import { AssetCategorySheet } from "./AssetCategorySheet";
import { DeleteAssetCategoryDialog } from "./DeleteAssetCategoryDialog";
import { MergeAssetCategoryDialog } from "./MergeAssetCategoryDialog";

type Step = CategoryStart | "merge";

type Props = {
  category: AssetCategory;
  start: CategoryStart;
  categories: AssetCategory[];
  onClose: () => void;
};

// Everything that can happen to one existing category: rename it, or delete it. A category in use cannot be deleted,
// so that path says so first and then moves its assets to another category.
export function AssetCategoryDialogs({
  category,
  start,
  categories,
  onClose,
}: Props) {
  const [step, setStep] = useState<Step>(start);
  if (step === "edit")
    return <AssetCategorySheet category={category} onClose={onClose} />;
  if (step === "merge")
    return (
      <MergeAssetCategoryDialog
        category={category}
        categories={categories}
        onClose={onClose}
      />
    );
  if (category.assetCount > 0)
    return (
      <AssetCategoryInUseDialog
        category={category}
        onMove={() => setStep("merge")}
        onClose={onClose}
      />
    );
  return <DeleteAssetCategoryDialog category={category} onClose={onClose} />;
}
