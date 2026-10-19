import { DeleteDialog } from "@/ui";
import type { ProjectCategory } from "../api";
import { useDeleteProjectCategory } from "../hooks";

type Props = { category: ProjectCategory; onClose: () => void };

// Deleting a category nobody uses. One that is in use goes through the move dialogs instead.
export function DeleteProjectCategoryDialog({ category, onClose }: Props) {
  const remove = useDeleteProjectCategory();
  return (
    <DeleteDialog
      title={`Delete the ${category.name} category?`}
      noun="category"
      onDelete={() => remove.mutateAsync(category.id)}
      onClose={onClose}
    >
      <p>It will be removed from the category list and from the pickers.</p>
      <p>
        No projects use it, so nothing else changes. A category that is still in
        use cannot be deleted.
      </p>
    </DeleteDialog>
  );
}
