import type { Asset } from "../api/assets";
import type { AssetCategory } from "../api/categories";

export const anAsset = (over: Partial<Asset> = {}): Asset => ({
  id: "a1",
  name: "Behringer X32 mixing desk",
  categoryId: 1,
  categoryName: "Equipment",
  serialNumber: "BX32-88214",
  purchaseDate: "2023-03-12",
  purchaseCost: 42000,
  currentValue: 31500,
  status: "InUse",
  description: "Main desk.",
  createdAt: "2026-01-01T00:00:00Z",
  ...over,
});

export const anAssetCategory = (
  over: Partial<AssetCategory> = {},
): AssetCategory => ({
  id: 1,
  name: "Equipment",
  assetCount: 3,
  ...over,
});
