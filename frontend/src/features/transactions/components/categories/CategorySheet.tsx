import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Chip, Field, FormSheet, SegmentedField } from "@/ui";
import type { Category } from "../../api/categories";
import {
  categoryFieldMap,
  categorySchema,
  emptyCategory,
  NAME_MAX,
  toCategoryInput,
  type CategoryValues,
} from "../../model/categories";
import { useCreateCategory, useRenameCategory } from "../../hooks/mutations";
import { typeLabel, typeTone } from "../../model/labels";
import styles from "./CategorySheet.module.css";

type Props = { category?: Category; onClose: () => void };

const types = [
  { value: "Income", label: "Income", icon: "arrow-down" },
  { value: "Expense", label: "Expense", icon: "arrow-up" },
] as const;

// The sheet for a category: new, or (with `category`) renaming. The type is chosen once and cannot change.
export function CategorySheet({ category, onClose }: Props) {
  const create = useCreateCategory();
  const rename = useRenameCategory();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<CategoryValues>({
    resolver: zodResolver(categorySchema),
    defaultValues: category
      ? { name: category.name, categoryType: category.categoryType }
      : emptyCategory,
  });
  const type = useWatch({ control: form.control, name: "categoryType" });
  const { errors } = form.formState;

  async function save(values: CategoryValues) {
    setProblem(null);
    try {
      if (category)
        await rename.mutateAsync({ id: category.id, name: values.name.trim() });
      else await create.mutateAsync(toCategoryInput(values));
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
          ? "Only the name can be changed."
          : "A heading to file transactions under."
      }
      onClose={onClose}
      formId="category-form"
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
          placeholder="e.g. Offerings"
          maxLength={NAME_MAX}
          autoComplete="off"
          error={errors.name?.message}
          {...form.register("name")}
        />
        {category ? (
          <div>
            <p className={styles.label}>Type</p>
            <div className={styles.fixed}>
              <Chip tone={typeTone[category.categoryType]}>
                {typeLabel[category.categoryType]}
              </Chip>
            </div>
            <p className={styles.hint}>
              The type cannot be changed after the category is created.
            </p>
          </div>
        ) : (
          <div>
            <SegmentedField
              label="Type"
              options={types}
              value={type}
              onChange={(next) => form.setValue("categoryType", next)}
            />
            <p className={styles.hint}>This cannot be changed later.</p>
          </div>
        )}
      </div>
    </FormSheet>
  );
}
