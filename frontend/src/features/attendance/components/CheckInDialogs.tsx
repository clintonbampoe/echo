import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { DeleteDialog, Field, FormSheet } from "@/ui";
import type { CheckIn, Session } from "../api";
import {
  checkInSchema,
  timeOf,
  toCheckedInAt,
  type CheckInValues,
} from "../checkInForm";
import { dayMonth, sessionTitle } from "../day";
import { useRemoveCheckIn, useUpdateCheckIn } from "../hooks";

type Props = { checkIn: CheckIn; session: Session; onClose: () => void };

// Changes when someone arrived. The time can be changed, not taken away.
export function EditCheckInTimeSheet({ checkIn, session, onClose }: Props) {
  const update = useUpdateCheckIn();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<CheckInValues>({
    resolver: zodResolver(checkInSchema),
    defaultValues: { time: timeOf(checkIn.checkedInAt) },
  });

  async function save({ time }: CheckInValues) {
    setProblem(null);
    try {
      await update.mutateAsync({
        id: checkIn.id,
        checkedInAt: toCheckedInAt(session.date, time),
      });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, { checkedInAt: "time" }, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit check-in time"
      subtitle={`${checkIn.personName}, ${sessionTitle(session)}, ${dayMonth(session.date)}`}
      onClose={onClose}
      formId="check-in-time-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this time",
          message: problem.message,
        }
      }
    >
      <Field
        label="Checked in at"
        type="time"
        error={form.formState.errors.time?.message}
        {...form.register("time")}
      />
    </FormSheet>
  );
}

// Takes one attendance record away. The person is not touched, and can be marked present again.
export function RemoveCheckInDialog({
  checkIn,
  onClose,
}: Omit<Props, "session">) {
  const remove = useRemoveCheckIn();
  return (
    <DeleteDialog
      title={`Remove ${checkIn.personName} from this session?`}
      noun="record"
      confirmLabel="Remove record"
      cancelLabel="Keep record"
      onDelete={() => remove.mutateAsync(checkIn.id)}
      onClose={onClose}
    >
      <p>
        Only this attendance record is removed. They stay{" "}
        {checkIn.personKind === "Member" ? "a member" : "a visitor"}, and you
        can mark them present again.
      </p>
    </DeleteDialog>
  );
}
