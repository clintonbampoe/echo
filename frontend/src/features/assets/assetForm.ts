// What the asset form accepts, and the words shown when it does not. The server has the final say; this catches the
// obvious mistakes first. No React in this file.
import { z } from "zod";
import { amountProblem, required } from "@/lib/validation";
import type { Option } from "@/ui";
import type { Asset, AssetCategory, AssetInput, AssetUpdate } from "./api";
import { assetStatuses } from "./filters";
import { statusLabel } from "./labels";

export const NAME_MAX = 100;
export const SERIAL_MAX = 100;
export const DESCRIPTION_MAX = 2000;
export const MONEY_MAX = 1_000_000;

const COST = {
  min: 0,
  max: MONEY_MAX,
  empty: "Enter a cost between 0 and 1,000,000",
  range: "Enter a cost between 0 and 1,000,000",
};
const VALUE = {
  min: 0,
  max: MONEY_MAX,
  empty: "Enter a value between 0 and 1,000,000",
  range: "Enter a value between 0 and 1,000,000",
};

const money = (rule: typeof COST) =>
  z
    .string()
    .trim()
    .superRefine((v, ctx) => {
      const message = amountProblem(v, rule);
      if (message) ctx.addIssue({ code: "custom", message });
    });

// A purchase date already set can be changed, not removed: the API has no way to clear it.
export const assetSchema = (dateLocked = false) =>
  z
    .object({
      name: required("Enter an asset name").max(
        NAME_MAX,
        `Use ${NAME_MAX} characters or fewer`,
      ),
      categoryId: z.string().min(1, "Choose a category"),
      status: z.enum(assetStatuses as [string, ...string[]]),
      serialNumber: z
        .string()
        .max(SERIAL_MAX, `Use ${SERIAL_MAX} characters or fewer`),
      purchaseDate: z.string(),
      purchaseCost: money(COST),
      currentValue: money(VALUE),
      description: z
        .string()
        .max(DESCRIPTION_MAX, `Use ${DESCRIPTION_MAX} characters or fewer`),
    })
    .superRefine((v, ctx) => {
      if (dateLocked && !v.purchaseDate)
        ctx.addIssue({
          code: "custom",
          path: ["purchaseDate"],
          message: "A purchase date can be changed, not removed",
        });
    });

export type AssetValues = z.infer<ReturnType<typeof assetSchema>>;

// A new asset starts as active, with everything else to fill in.
export const emptyAsset: AssetValues = {
  name: "",
  categoryId: "",
  status: "Active",
  serialNumber: "",
  purchaseDate: "",
  purchaseCost: "",
  currentValue: "",
  description: "",
};

const asMoney = (n: number) =>
  n.toLocaleString("en-US", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

export function fromAsset(a: Asset): AssetValues {
  return {
    name: a.name,
    categoryId: String(a.categoryId),
    status: a.status,
    serialNumber: a.serialNumber ?? "",
    purchaseDate: a.purchaseDate?.slice(0, 10) ?? "",
    purchaseCost: asMoney(a.purchaseCost),
    currentValue: asMoney(a.currentValue),
    description: a.description ?? "",
  };
}

const number = (text: string) => Number(text.replace(/,/g, ""));

export function toAssetInput(v: AssetValues): AssetInput {
  return {
    categoryId: Number(v.categoryId),
    name: v.name.trim(),
    status: v.status as AssetInput["status"],
    serialNumber: v.serialNumber.trim() || undefined,
    purchaseDate: v.purchaseDate || undefined,
    purchaseCost: number(v.purchaseCost),
    currentValue: number(v.currentValue),
    description: v.description.trim() || undefined,
  };
}

// Saving changes: an emptied serial number or description is sent as empty text, because leaving it out would keep
// the old one.
export const toAssetUpdate = (v: AssetValues): AssetUpdate => ({
  ...toAssetInput(v),
  serialNumber: v.serialNumber.trim(),
  description: v.description.trim(),
});

export const categoryChoices = (
  categories: AssetCategory[] | undefined,
): readonly Option[] =>
  (categories ?? []).map((c) => ({ value: String(c.id), label: c.name }));

export const statusChoices: readonly Option[] = assetStatuses.map((s) => ({
  value: s,
  label: statusLabel[s],
}));

// The API reports "categoryId", "purchaseCost" and so on; the form uses the same names.
export const assetFieldMap = Object.fromEntries(
  Object.keys(emptyAsset).map((k) => [k, k]),
) as Record<keyof AssetValues, keyof AssetValues>;
