import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { MemberMatch } from "@/features/members";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { EventItem } from "../../api/events";
import {
  eventFieldMap,
  eventSchema,
  fromEvent,
  lockedFor,
  toEventUpdate,
  type EventValues,
} from "../../forms/schema";
import { useUpdateEvent } from "../../hooks/mutations";
import { EventFields } from "./EventFields";

type Props = { event: EventItem; onClose: () => void };

// The "Edit" sheet: the same form as New event, filled with what the event has now.
// A time or the limit that is set can be changed, not removed: the server has no way to clear them.
export function EditEventSheet({ event, onClose }: Props) {
  const update = useUpdateEvent(event.id);
  const [organizer, setOrganizer] = useState<
    (Pick<MemberMatch, "id" | "name"> & { phoneNumber?: string }) | null
  >({ id: event.organizerId, name: event.organizerName });
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<EventValues>({
    resolver: zodResolver(eventSchema(lockedFor(event))),
    defaultValues: fromEvent(event),
  });

  function choose(next: MemberMatch | null) {
    setOrganizer(next);
    form.setValue("organizerId", next?.id ?? "", {
      shouldValidate: true,
      shouldDirty: true,
    });
  }

  async function save(values: EventValues) {
    setProblem(null);
    try {
      await update.mutateAsync(toEventUpdate(values));
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, eventFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title={`Edit ${event.name}`}
      subtitle="Registrations and check-ins stay as they are."
      onClose={onClose}
      formId="event-edit-form"
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
      <EventFields form={form} organizer={organizer} onOrganizer={choose} />
    </FormSheet>
  );
}
