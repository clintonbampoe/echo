import type { Asset } from "../api";
import { DeleteAssetDialog } from "./DeleteAssetDialog";
import { EditAssetSheet } from "./EditAssetSheet";

// Where an existing asset can be opened at: to change, or to delete.
export type AssetStart = "edit" | "delete";

type Props = { asset: Asset; start: AssetStart; onClose: () => void };

// What can happen to one existing asset from the list's ⋯ menu.
export function AssetDialogs({ asset, start, onClose }: Props) {
  return start === "delete" ? (
    <DeleteAssetDialog asset={asset} onClose={onClose} />
  ) : (
    <EditAssetSheet asset={asset} onClose={onClose} />
  );
}
