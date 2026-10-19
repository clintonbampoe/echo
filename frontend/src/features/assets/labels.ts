import type { ChipTone } from "@/ui";
import type { AssetStatus } from "./api";

export const statusLabel: Record<AssetStatus, string> = {
  Active: "Active",
  InUse: "In use",
  InStorage: "In storage",
  UnderMaintenance: "Under maintenance",
  Liquidated: "Liquidated",
};

export const statusTone: Record<AssetStatus, ChipTone> = {
  Active: "positive",
  InUse: "positive",
  InStorage: "neutral",
  UnderMaintenance: "warning",
  Liquidated: "neutral",
};
