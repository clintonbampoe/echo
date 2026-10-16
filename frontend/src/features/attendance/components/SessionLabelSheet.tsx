import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { ControlledSelect, Field, FormSheet } from "@/ui";
import type { SessionLabel } from "../api";
import { useCreateSessionLabel, useUpdateSessionLabel } from "../hooks";
import {
  categoryOptions,
  emptyLabel,
  LABEL_MAX,
  labelFieldMap,
  labelSchema,
  toLabelInput,
  type LabelValues,
} from "../sessionForm";

type Props = { label?: SessionLabel; onClose: () => void };

// The sheet for a session type: new, or (with `label`) renaming. The category is chosen once and cannot change.
export function SessionLabelSheet({ label, onClose }: Props) {
  const create = useCreateSessionLabel();
  const update = useUpdateSessionLabel();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<LabelValues>({
    resolver: zodResolver(labelSchema),
    defaultValues: label
      ? { name: label.name, category: label.category }
      : emptyLabel,
  });
  const { errors } = form.formState;

  async function save(values: LabelValues) {
    setProblem(null);
    try {
      if (label)
        await update.mutateAsync({ id: label.id, name: values.name.trim() });
      else await create.mutateAsync(toLabelInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, labelFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={label ? `Edit ${label.name}` : "New session type"}
      subtitle={
        label
          ? "Only the name can be changed."
          : "The kind of gathering you hold regularly."
      }
      onClose={onClose}
      formId="session-type-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel={label ? "Save changes" : "Create session type"}
      pending={create.isPending || update.isPending}
      failures={Object.keys(errors).length}
      problem={
        problem && {
          title: "We could not save this session type",
          message: problem.message,
        }
      }
    >
      <Field
        label="Name"
        maxLength={LABEL_MAX}
        autoComplete="off"
        error={errors.name?.message}
        {...form.register("name")}
      />
      {label ? (
        <Field
          label="Category"
          value={label.category}
          readOnly
          hint="Category cannot be changed after the type is created."
        />
      ) : (
        <ControlledSelect
          control={form.control}
          name="category"
          label="Category"
          options={categoryOptions}
        />
      )}
    </FormSheet>
  );
}
