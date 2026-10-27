import { DeleteDialog } from "@/ui";
import type { AssetCategory } from "../../api/categories";
import { useDeleteAssetCategory } from "../../hooks/mutations";

type Props = { category: AssetCategory; onClose: () => void };

// Deleting a category nobody uses. One that is in use goes through the move dialogs instead.
export function DeleteAssetCategoryDialog({ category, onClose }: Props) {
  const remove = useDeleteAssetCategory();
  return (
    <DeleteDialog
      title={`Delete the ${category.name} category?`}
      noun="category"
      onDelete={() => remove.mutateAsync(category.id)}
      onClose={onClose}
    >
      <p>It will be removed from the category list and the pickers.</p>
      <p>
        No assets use it, so nothing else changes. A category that is still in
        use cannot be deleted.
      </p>
    </DeleteDialog>
  );
}
