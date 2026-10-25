import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import {
  assetFieldMap,
  assetSchema,
  emptyAsset,
  toAssetInput,
  type AssetValues,
} from "../../forms/schema";
import { useAssetCategories } from "../../hooks/queries";
import { useCreateAsset } from "../../hooks/mutations";
import { AssetFields } from "./AssetFields";

type Props = { open: boolean; onClose: () => void };

// The "New asset" sheet. The form only exists while the sheet is open, so it is blank every time.
export function AssetSheet({ open, onClose }: Props) {
  return open ? <NewAsset onClose={onClose} /> : null;
}

function NewAsset({ onClose }: { onClose: () => void }) {
  const create = useCreateAsset();
  const categories = useAssetCategories();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<AssetValues>({
    resolver: zodResolver(assetSchema()),
    defaultValues: emptyAsset,
  });

  async function save(values: AssetValues) {
    setProblem(null);
    try {
      await create.mutateAsync(toAssetInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, assetFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New asset"
      subtitle="Current value is entered by hand. Echo does not depreciate it."
      onClose={onClose}
      formId="asset-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create asset"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this asset",
          message: problem.message,
        }
      }
    >
      <AssetFields form={form} categories={categories.data} />
    </FormSheet>
  );
}
