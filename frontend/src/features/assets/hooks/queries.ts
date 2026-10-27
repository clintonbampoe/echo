import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import type { AssetFilters } from "../api/assets";
import { getAsset, getAssetSummary, listAssets } from "../api/assets";
import { listAssetCategories } from "../api/categories";
import { assetKeys } from "./keys";

const FIVE_MINUTES = 5 * 60_000;

// Every asset that matches the filters, by name.
export const useAssets = (filters: AssetFilters) =>
  usePagedList(assetKeys.list(filters), (c, s) => listAssets(filters, c, s));

export const useAssetSummary = (filters: AssetFilters) =>
  useQuery({
    queryKey: assetKeys.summary(filters),
    queryFn: ({ signal }) => getAssetSummary(filters, signal),
  });

// One asset, for its own page.
export const useAsset = (id: string) =>
  useQuery({
    queryKey: assetKeys.one(id),
    queryFn: ({ signal }) => getAsset(id, signal),
  });

// Every category, for the filter and the form. They change rarely, so they are kept for a few minutes.
export const useAssetCategories = () =>
  useQuery({
    queryKey: assetKeys.categories,
    queryFn: ({ signal }) => listAssetCategories(signal),
    staleTime: FIVE_MINUTES,
  });
