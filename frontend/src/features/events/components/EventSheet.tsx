import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { MemberMatch } from "@/features/members";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import {
  emptyEvent,
  eventFieldMap,
  eventSchema,
  toEventInput,
  type EventValues,
} from "../eventForm";
import { useCreateEvent } from "../hooks";
import { EventFields } from "./EventFields";

type Props = { open: boolean; onClose: () => void };

// The "New event" sheet. The form only exists while the sheet is open, so it is blank every time.
export function EventSheet({ open, onClose }: Props) {
  return open ? <NewEvent onClose={onClose} /> : null;
}

function NewEvent({ onClose }: { onClose: () => void }) {
  const create = useCreateEvent();
  const [organizer, setOrganizer] = useState<MemberMatch | null>(null);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<EventValues>({
    resolver: zodResolver(eventSchema()),
    defaultValues: emptyEvent,
  });

  function choose(next: MemberMatch | null) {
    setOrganizer(next);
    form.setValue("organizerId", next?.id ?? "", {
      shouldValidate: form.formState.isSubmitted,
    });
  }

  async function save(values: EventValues) {
    setProblem(null);
    try {
      await create.mutateAsync(toEventInput(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, eventFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="New event"
      subtitle="A one-off gathering your church is planning."
      onClose={onClose}
      formId="event-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Create event"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this event",
          message: problem.message,
        }
      }
    >
      <EventFields form={form} organizer={organizer} onOrganizer={choose} />
    </FormSheet>
  );
}
