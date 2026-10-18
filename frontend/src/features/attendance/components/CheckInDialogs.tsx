import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { ControlledTime, DeleteDialog, FormSheet } from "@/ui";
import type { CheckIn } from "../api";
import {
  checkInSchema,
  timeOf,
  toCheckedInAt,
  type CheckInValues,
} from "../checkInForm";
import { dayMonth, recordTitle } from "../day";
import { useRemoveCheckIn, useUpdateCheckIn } from "../hooks";

type Props = { checkIn: CheckIn; onClose: () => void };

// Changes when someone arrived. The time can be changed, not taken away.
export function EditCheckInTimeSheet({ checkIn, onClose }: Props) {
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
        checkedInAt: toCheckedInAt(checkIn.sessionDate, time),
      });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, { checkedInAt: "time" }, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit check-in time"
      subtitle={`${checkIn.personName}, ${recordTitle(checkIn)}, ${dayMonth(checkIn.sessionDate)}`}
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
      <ControlledTime
        control={form.control}
        name="time"
        label="Checked in at"
      />
    </FormSheet>
  );
}

// Takes one attendance record away. The person is not touched, and can be marked present again.
export function RemoveCheckInDialog({ checkIn, onClose }: Props) {
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
