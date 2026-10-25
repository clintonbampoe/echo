import type { Category } from "../api";
import { CategorySheet } from "./CategorySheet";
import type { CategoryStart } from "./CategoryList";
import { MergeCategoryDialog } from "./MergeCategoryDialog";
import { DeleteCategoryDialog } from "./DeleteCategoryDialog";

type Props = {
  category: Category;
  start: CategoryStart;
  categories: Category[];
  onClose: () => void;
};

// Everything that can happen to one existing category: rename it, or delete it. A category in use cannot be deleted,
// so that path says so and moves its transactions to another category in the same step.
export function CategoryDialogs({
  category,
  start,
  categories,
  onClose,
}: Props) {
  if (start === "edit")
    return <CategorySheet category={category} onClose={onClose} />;
  if (category.transactionCount > 0)
    return (
      <MergeCategoryDialog
        category={category}
        categories={categories}
        onClose={onClose}
      />
    );
  return <DeleteCategoryDialog category={category} onClose={onClose} />;
}
