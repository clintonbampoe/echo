import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { ControlledTime, FormSheet } from "@/ui";
import type { EventCheckIn } from "../../api/checkIns";
import { useUpdateCheckInTime } from "../../hooks/mutations";

const schema = z.object({
  time: z.string().min(1, "Enter the time they arrived"),
});
type Values = z.infer<typeof schema>;

type Props = { checkIn: EventCheckIn; eventName: string; onClose: () => void };

// Changes when someone arrived. The time can be changed, not taken away.
export function EditCheckInTimeSheet({ checkIn, eventName, onClose }: Props) {
  const update = useUpdateCheckInTime();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { time: checkIn.checkInTime.slice(0, 5) },
  });

  async function save({ time }: Values) {
    setProblem(null);
    try {
      await update.mutateAsync({ id: checkIn.id, time: `${time}:00` });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, { checkInTime: "time" }, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit check-in time"
      subtitle={`${checkIn.memberName}, ${eventName}`}
      onClose={onClose}
      formId="event-check-in-time-form"
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
