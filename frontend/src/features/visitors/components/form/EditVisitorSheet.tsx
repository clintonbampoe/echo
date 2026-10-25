import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Visitor } from "../../api/visitors";
import { useUpdateVisitor } from "../../hooks";
import {
  fieldMap,
  fromVisitor,
  toInput,
  visitorSchema,
  type VisitorValues,
} from "../../forms/schema";
import { VisitorFields } from "./VisitorFields";

type Props = { visitor: Visitor; onClose: () => void };

// The "Edit" sheet: the same form as New visitor, filled with what the visitor has now.
// Note: the server treats an empty optional field as "leave unchanged", so it cannot clear one.
export function EditVisitorSheet({ visitor, onClose }: Props) {
  const update = useUpdateVisitor(visitor.id);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<VisitorValues>({
    resolver: zodResolver(visitorSchema),
    defaultValues: fromVisitor(visitor),
  });

  async function save(values: VisitorValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={`Edit ${visitor.name}`}
      subtitle="Changes save to this visitor only."
      onClose={onClose}
      formId="visitor-edit-form"
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
      <VisitorFields form={form} />
    </FormSheet>
  );
}
