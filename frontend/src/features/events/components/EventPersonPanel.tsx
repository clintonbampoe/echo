import { formatClock, formatDayMonth } from "@/lib/format";
import { paths } from "@/lib/paths";
import { PersonRecord, RecordActions, Sheet } from "@/ui";
import {
  missingText,
  panelTitle,
  signedUpText,
  type EventPerson,
} from "../eventPerson";
import type { Phase } from "../roster";

type Props = {
  person: EventPerson;
  eventName: string;
  phase: Phase;
  onClose: () => void;
  // Each one closes the panel and opens its own dialog or sheet on the page behind.
  onEditTime: () => void;
  onRemoveRegistration: () => void;
  onRemoveCheckIn: () => void;
};

// A person shown on an event, opened from either roster tab. Everything here is about this event;
// the person's own details are on their member page, one deliberate link away (design rule S6).
export function EventPersonPanel({
  person,
  eventName,
  phase,
  onClose,
  onEditTime,
  onRemoveRegistration,
  onRemoveCheckIn,
}: Props) {
  const { registration, checkIn } = person;
  const remove = [
    ...(checkIn
      ? [{ label: "Delete check-in", onClick: onRemoveCheckIn }]
      : []),
    ...(registration
      ? [{ label: "Remove registration", onClick: onRemoveRegistration }]
      : []),
  ];
  return (
    <Sheet
      open
      onClose={onClose}
      title={panelTitle(person)}
      footer={
        <RecordActions
          edit={
            checkIn
              ? { label: "Edit check-in time", onClick: onEditTime }
              : undefined
          }
          remove={remove}
        />
      }
    >
      <PersonRecord
        name={person.name}
        meta={`${person.registered ? "Registered" : "Walk-in"} · ${
          checkIn ? "Checked in" : missingText(phase)
        }`}
        rows={[
          ["Event", eventName],
          ["Registered", signedUpText(person, (day) => formatDayMonth(day))],
          [
            "Checked in",
            checkIn ? formatClock(checkIn.checkInTime) : missingText(phase),
          ],
        ]}
        profileHref={paths.member(person.memberId, "events")}
        profileNote={`${person.name} · opens the Events tab`}
      />
    </Sheet>
  );
}
