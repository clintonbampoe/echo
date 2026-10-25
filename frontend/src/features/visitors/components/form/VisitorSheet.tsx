import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import { useCreateVisitor } from "../../hooks";
import {
  emptyVisitor,
  fieldMap,
  toInput,
  visitorSchema,
  type VisitorValues,
} from "../../forms/schema";
import { VisitorFields } from "./VisitorFields";

type Props = { open: boolean; onClose: () => void };

// The "New visitor" sheet. The form only exists while the sheet is open, so it is blank every time.
export function VisitorSheet({ open, onClose }: Props) {
  return open ? <NewVisitor onClose={onClose} /> : null;
}

function NewVisitor({ onClose }: { onClose: () => void }) {
  const create = useCreateVisitor();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<VisitorValues>({
    resolver: zodResolver(visitorSchema),
    defaultValues: emptyVisitor,
  });

  async function save(values: VisitorValues) {
    setProblem(null);
    try {
      await create.mutateAsync(toInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, fieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New visitor"
      subtitle="Only a name is required. You can add the rest later."
      onClose={onClose}
      formId="visitor-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create visitor"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this visitor",
          message: problem.message,
        }
      }
    >
      <VisitorFields form={form} />
    </FormSheet>
  );
}
