import { DeleteDialog } from "@/ui";
import type { Category } from "../api";
import { useDeleteCategory } from "../hooks";

type Props = { category: Category; onClose: () => void };

// Deleting a category nobody uses. One that is in use goes through the move dialogs instead.
export function DeleteCategoryDialog({ category, onClose }: Props) {
  const remove = useDeleteCategory();
  return (
    <DeleteDialog
      title={`Delete the ${category.name} category?`}
      noun="category"
      onDelete={() => remove.mutateAsync(category.id)}
      onClose={onClose}
    >
      <p>It will be removed from the category list and from the pickers.</p>
      <p>
        No transactions use it, so nothing else changes. A category that is
        still in use cannot be deleted.
      </p>
    </DeleteDialog>
  );
}
