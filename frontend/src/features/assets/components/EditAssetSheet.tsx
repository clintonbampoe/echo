import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Asset } from "../api";
import {
  assetFieldMap,
  assetSchema,
  fromAsset,
  toAssetUpdate,
  type AssetValues,
} from "../assetForm";
import { useAssetCategories, useUpdateAsset } from "../hooks";
import { AssetFields } from "./AssetFields";

type Props = { asset: Asset; onClose: () => void };

// The "Edit" sheet: the same form as New asset, filled with what the asset has now. A purchase date that is set can be
// changed, not removed: the server has no way to clear it.
export function EditAssetSheet({ asset, onClose }: Props) {
  const update = useUpdateAsset(asset.id);
  const categories = useAssetCategories();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<AssetValues>({
    resolver: zodResolver(assetSchema(Boolean(asset.purchaseDate))),
    defaultValues: fromAsset(asset),
  });

  async function save(values: AssetValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toAssetUpdate(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, assetFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit asset"
      subtitle="Changing the status or value updates the totals."
      onClose={onClose}
      formId="asset-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      note={form.formState.isDirty ? "Unsaved changes" : undefined}
      problem={
        problem && {
          title: "We could not save these changes",
          message: problem.message,
        }
      }
    >
      <AssetFields form={form} categories={categories.data} />
    </FormSheet>
  );
}
