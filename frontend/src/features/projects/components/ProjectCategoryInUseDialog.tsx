import { ConfirmDialog, Notice } from "@/ui";
import type { ProjectCategory } from "../api";
import { plural } from "../categoryView";

type Props = {
  category: ProjectCategory;
  onMove: () => void;
  onClose: () => void;
};

// A category with projects cannot be deleted. Says so, and offers the way out: move them to another category.
export function ProjectCategoryInUseDialog({
  category,
  onMove,
  onClose,
}: Props) {
  const n = category.projectCount;
  return (
    <ConfirmDialog
      title={`${category.name} is still in use`}
      confirmLabel={`Move ${plural(n, "project")}`}
      cancelLabel="Cancel"
      tone="neutral"
      onConfirm={onMove}
      onCancel={onClose}
    >
      <p>
        {n === 1 ? "1 project is" : `${n} projects are`} filed under{" "}
        {category.name}, so it cannot be deleted yet. Move{" "}
        {n === 1 ? "it" : "them"} to another category, then it can be deleted.
      </p>
      <Notice
        tone="warning"
        title="Moving keeps every project as it is. Only the category changes."
      />
    </ConfirmDialog>
  );
}
