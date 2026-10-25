import { DeleteDialog, Notice } from "@/ui";
import type { Asset } from "../../api/assets";
import { useDeleteAsset } from "../../hooks/mutations";

// onDeleted runs after a successful delete (the asset's page uses it to go back to the list).
type Props = {
  asset: Pick<Asset, "id" | "name">;
  onClose: () => void;
  onDeleted?: () => void;
};

// "Delete" removes the asset from the list and the totals. An asset the church has sold or scrapped is better kept and
// marked Liquidated, which the dialog says.
export function DeleteAssetDialog({ asset, onClose, onDeleted }: Props) {
  const remove = useDeleteAsset();
  return (
    <DeleteDialog
      title={`Delete ${asset.name}?`}
      noun="asset"
      onDelete={() => remove.mutateAsync(asset.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>It will be removed from the assets list and from the totals.</p>
      <Notice title="The record is kept in history. If the church has sold or scrapped it, set the status to Liquidated instead and keep it listed." />
    </DeleteDialog>
  );
}
