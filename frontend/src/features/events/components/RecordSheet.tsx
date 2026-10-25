import { Button, Sheet } from "@/ui";
import type { EventItem } from "../api";
import type { Mode } from "../record";
import { RecordPanel } from "../RecordPanel";

type Props = {
  event: EventItem;
  mode: Mode;
  // Who has signed up and who has come already, to mark them in the search.
  registered: ReadonlySet<string>;
  checkedIn: ReadonlySet<string>;
  onClose: () => void;
};

// Registering and checking in, in a panel that slides over the event: search for a member, then tap. Each tap is
// saved at once, so there is nothing to confirm; Done just closes it.
export function RecordSheet({ event, mode, onClose, ...who }: Props) {
  return (
    <Sheet
      open
      onClose={onClose}
      title={mode === "check-in" ? "Check in" : "Register"}
      subtitle={event.name}
      footer={<Button onClick={onClose}>Done</Button>}
    >
      <RecordPanel event={event} initial={mode} {...who} />
    </Sheet>
  );
}
