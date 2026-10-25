import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Tithe } from "../../api/tithes";
import { useUpdateTithe } from "../../hooks/mutations";
import {
  fromTithe,
  titheFieldMap,
  titheSchema,
  toTitheUpdate,
  type TitheValues,
} from "../../forms/schema";
import { TitheFields } from "./TitheFields";

type Props = {
  tithe: Tithe;
  today: string;
  onClose: () => void;
  // Opened from the "already has a tithe" notice: the day starts as today, and the sheet says to enter the new total.
  topUp?: boolean;
};

// The "Edit tithe" sheet: the same form as Record tithe, filled with what was recorded. The member is shown but
// cannot be changed: a tithe recorded for the wrong person is deleted and recorded again.
export function EditTitheSheet({ tithe, today, onClose, topUp }: Props) {
  const update = useUpdateTithe();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<TitheValues>({
    resolver: zodResolver(titheSchema(today)),
    defaultValues: topUp
      ? { ...fromTithe(tithe), collectionDate: today }
      : fromTithe(tithe),
  });

  async function save(values: TitheValues) {
    setProblem(null);
    try {
      await update.mutateAsync({ id: tithe.id, ...toTitheUpdate(values) });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, titheFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit tithe"
      subtitle={
        topUp
          ? "Enter the new total for the month. The day is today."
          : "Changes update the totals straight away."
      }
      onClose={onClose}
      formId="tithe-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this tithe",
          message: problem.message,
        }
      }
    >
      <TitheFields
        form={form}
        today={today}
        member={{ id: tithe.memberId, name: tithe.memberName }}
      />
    </FormSheet>
  );
}
