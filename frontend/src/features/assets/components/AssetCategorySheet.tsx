import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Field, FormSheet } from "@/ui";
import type { AssetCategory } from "../api";
import {
  categoryFieldMap,
  categorySchema,
  CATEGORY_NAME_MAX,
  emptyCategory,
  type CategoryValues,
} from "../categoryView";
import { useCreateAssetCategory, useRenameAssetCategory } from "../hooks";
import styles from "./AssetCategorySheet.module.css";

type Props = { category?: AssetCategory; onClose: () => void };

// The sheet for a category: new, or (with `category`) renaming it.
export function AssetCategorySheet({ category, onClose }: Props) {
  const create = useCreateAssetCategory();
  const rename = useRenameAssetCategory();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<CategoryValues>({
    resolver: zodResolver(categorySchema),
    defaultValues: category ? { name: category.name } : emptyCategory,
  });
  const { errors } = form.formState;

  async function save(values: CategoryValues) {
    setProblem(null);
    try {
      if (category)
        await rename.mutateAsync({ id: category.id, name: values.name.trim() });
      else await create.mutateAsync(values.name.trim());
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, categoryFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={category ? `Edit ${category.name}` : "New category"}
      subtitle={
        category
          ? "Assets keep the category, with the new name."
          : "A heading to group assets under."
      }
      onClose={onClose}
      formId="asset-category-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel={category ? "Save changes" : "Create category"}
      pending={create.isPending || rename.isPending}
      failures={Object.keys(errors).length}
      problem={
        problem && {
          title: "We could not save this category",
          message: problem.message,
        }
      }
    >
      <div className={styles.fields}>
        <Field
          label="Name"
          placeholder="e.g. Equipment"
          maxLength={CATEGORY_NAME_MAX}
          autoComplete="off"
          error={errors.name?.message}
          {...form.register("name")}
        />
      </div>
    </FormSheet>
  );
}
