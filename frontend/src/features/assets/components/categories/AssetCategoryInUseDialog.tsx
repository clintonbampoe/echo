import { plural } from "@/lib/format";
import { ConfirmDialog, Notice } from "@/ui";
import type { AssetCategory } from "../../api/categories";

type Props = {
  category: AssetCategory;
  onMove: () => void;
  onClose: () => void;
};

// A category with assets cannot be deleted. Says so, and offers the way out: move them to another category.
export function AssetCategoryInUseDialog({ category, onMove, onClose }: Props) {
  const n = category.assetCount;
  return (
    <ConfirmDialog
      title={`${category.name} is still in use`}
      confirmLabel={`Move ${plural(n, "asset")}`}
      cancelLabel="Cancel"
      onConfirm={onMove}
      onCancel={onClose}
    >
      <p>
        {n === 1 ? "1 asset is" : `${plural(n, "asset")} are`} filed under{" "}
        {category.name}, so it cannot be deleted yet. Move{" "}
        {n === 1 ? "it" : "them"} to another category, then it can be deleted.
      </p>
      <Notice
        tone="warning"
        title="Moving keeps every asset as it is. Only the category changes."
      />
    </ConfirmDialog>
  );
}
