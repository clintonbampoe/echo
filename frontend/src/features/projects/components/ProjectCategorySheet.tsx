import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Field, FormSheet } from "@/ui";
import type { ProjectCategory } from "../api";
import {
  categoryFieldMap,
  categorySchema,
  CATEGORY_NAME_MAX,
  emptyCategory,
  type CategoryValues,
} from "../categoryView";
import { useCreateProjectCategory, useRenameProjectCategory } from "../hooks";
import styles from "./ProjectCategorySheet.module.css";

type Props = { category?: ProjectCategory; onClose: () => void };

// The sheet for a category: new, or (with `category`) renaming it.
export function ProjectCategorySheet({ category, onClose }: Props) {
  const create = useCreateProjectCategory();
  const rename = useRenameProjectCategory();
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
          ? "Projects filed under it show the new name."
          : "A heading to group projects under."
      }
      onClose={onClose}
      formId="project-category-form"
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
          placeholder="e.g. Building"
          maxLength={CATEGORY_NAME_MAX}
          autoComplete="off"
          error={errors.name?.message}
          {...form.register("name")}
        />
      </div>
    </FormSheet>
  );
}
