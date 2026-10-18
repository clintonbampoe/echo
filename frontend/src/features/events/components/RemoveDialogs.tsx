import { DeleteDialog } from "@/ui";
import type { EventCheckIn, EventRegistration } from "../api";
import { useRemoveCheckIn, useRemoveRegistration } from "../hooks";

type Props<T> = { record: T; onClose: () => void };

// Takes a sign-up away. The member is not touched, and can register again. A check-in they already have stays.
export function RemoveRegistrationDialog({
  record,
  onClose,
}: Props<EventRegistration>) {
  const remove = useRemoveRegistration();
  return (
    <DeleteDialog
      title={`Remove ${record.memberName}'s registration?`}
      noun="registration"
      confirmLabel="Remove registration"
      cancelLabel="Keep registration"
      onDelete={() => remove.mutateAsync(record.id)}
      onClose={onClose}
    >
      <p>
        {record.memberName} will no longer be signed up for this event. They can
        register again.
      </p>
      <p>If they have already checked in, that check-in is kept.</p>
    </DeleteDialog>
  );
}

// Takes one check-in away. The member is not touched, and can be checked in again.
export function RemoveCheckInDialog({ record, onClose }: Props<EventCheckIn>) {
  const remove = useRemoveCheckIn();
  return (
    <DeleteDialog
      title={`Remove ${record.memberName}'s check-in?`}
      noun="check-in"
      confirmLabel="Remove check-in"
      cancelLabel="Keep check-in"
      onDelete={() => remove.mutateAsync(record.id)}
      onClose={onClose}
    >
      <p>
        {record.memberName} will no longer show as having come. They can be
        checked in again.
      </p>
      <p>Their registration, if they have one, is kept.</p>
    </DeleteDialog>
  );
}
