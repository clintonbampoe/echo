import { useState } from "react";
import type { Category } from "../api";
import { CategorySheet } from "./CategorySheet";
import type { CategoryStart } from "./CategoryList";
import { MergeCategoryDialog } from "./MergeCategoryDialog";
import { InUseDialog } from "./InUseDialog";
import { DeleteCategoryDialog } from "./DeleteCategoryDialog";

type Step = CategoryStart | "merge";

type Props = {
  category: Category;
  start: CategoryStart;
  categories: Category[];
  onClose: () => void;
};

// Everything that can happen to one existing category: rename it, or delete it. A category in use cannot be deleted,
// so that path says so first and then moves its transactions to another category.
export function CategoryDialogs({
  category,
  start,
  categories,
  onClose,
}: Props) {
  const [step, setStep] = useState<Step>(start);
  if (step === "edit")
    return <CategorySheet category={category} onClose={onClose} />;
  if (step === "merge")
    return (
      <MergeCategoryDialog
        category={category}
        categories={categories}
        onClose={onClose}
      />
    );
  if (category.transactionCount > 0)
    return (
      <InUseDialog
        category={category}
        onMove={() => setStep("merge")}
        onClose={onClose}
      />
    );
  return <DeleteCategoryDialog category={category} onClose={onClose} />;
}
