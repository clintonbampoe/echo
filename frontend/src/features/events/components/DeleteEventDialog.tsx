import { formatNumber } from "@/lib/format";
import { DeleteDialog } from "@/ui";
import type { EventItem } from "../api";
import { useDeleteEvent } from "../hooks";

// onDeleted runs after a successful delete (the page uses it to go back to the list). `checkIns` is how many came,
// when the page knows.
type Props = {
  event: Pick<EventItem, "id" | "name" | "registrationCount">;
  checkIns?: number;
  onClose: () => void;
  onDeleted?: () => void;
};

const count = (n: number, one: string, many: string) =>
  `${formatNumber(n)} ${n === 1 ? one : many}`;

// "Delete" removes the event from the lists and search. Who signed up and who came is kept, and members' own pages
// still show the event by name.
export function DeleteEventDialog({
  event,
  checkIns,
  onClose,
  onDeleted,
}: Props) {
  const remove = useDeleteEvent();
  const kept =
    checkIns === undefined
      ? "registrations and check-ins"
      : `${count(event.registrationCount, "registration", "registrations")} and ${count(checkIns, "check-in", "check-ins")}`;
  return (
    <DeleteDialog
      title={`Delete ${event.name}?`}
      noun="event"
      onDelete={() => remove.mutateAsync(event.id)}
      onClose={onClose}
      onDeleted={onDeleted}
    >
      <p>The event will be removed from your lists and from search.</p>
      <p>
        Its {kept} are kept, and members' histories still show this event by
        name.
      </p>
    </DeleteDialog>
  );
}
