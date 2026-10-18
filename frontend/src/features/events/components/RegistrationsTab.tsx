import { useState } from "react";
import { formatClock, formatDayTime } from "@/lib/format";
import {
  Chip,
  FilterBar,
  FilterSelect,
  ListBody,
  PersonList,
  type PersonColumn,
} from "@/ui";
import type { EventCheckIn, EventRegistration } from "../api";
import { filterGuests, guestStatus, type Guest, type Phase } from "../roster";
import { EditCheckInTimeSheet } from "./EditCheckInTimeSheet";
import { guestPerson } from "../eventPerson";
import { EventPersonPanel } from "./EventPersonPanel";
import { rosterList, type RosterState } from "./rosterList";
import { RemoveCheckInDialog, RemoveRegistrationDialog } from "./RemoveDialogs";

type Props = {
  guests: Guest[];
  eventName: string;
  phase: Phase;
  state: RosterState;
};

// Gone without coming is a "no-show" only once the event is over; before that they have just not arrived yet.
const missingLabel = (phase: Phase) =>
  phase === "over" ? "No-show" : "Not checked in";

const statusOptions = (phase: Phase) => [
  { value: "", label: "Status" },
  { value: "came", label: "Checked in" },
  { value: "missing", label: missingLabel(phase) },
];

const columns = (phase: Phase): PersonColumn<Guest>[] => [
  {
    header: "Signed up",
    width: "minmax(140px, 1fr)",
    cell: (g) => formatDayTime(g.registration.registrationDate),
  },
  {
    header: "Status",
    width: "minmax(150px, 1.2fr)",
    chip: true,
    cell: (g) =>
      guestStatus(g) === "came" && g.checkIn ? (
        <Chip tone="positive">
          Checked in {formatClock(g.checkIn.checkInTime)}
        </Chip>
      ) : (
        <Chip tone={phase === "over" ? "warning" : "neutral"}>
          {missingLabel(phase)}
        </Chip>
      ),
  },
];

// Who signed up for the event, and whether they came. Searched by name and narrowed by status here in the
// browser: the server has no search for them, and the page already holds every sign-up.
// A row opens that person's panel for this event (design rule S6); every action lives in there.
export function RegistrationsTab({ guests, eventName, phase, state }: Props) {
  const [name, setName] = useState("");
  const [status, setStatus] = useState("");
  const [resetKey, setResetKey] = useState(0);
  const [viewing, setViewing] = useState<Guest | null>(null);
  const [editing, setEditing] = useState<EventCheckIn | null>(null);
  const [removing, setRemoving] = useState<EventRegistration | null>(null);
  const [removingCheckIn, setRemovingCheckIn] = useState<EventCheckIn | null>(
    null,
  );
  const filtered = name !== "" || status !== "";

  function clear() {
    setName("");
    setStatus("");
    setResetKey(resetKey + 1);
  }

  // The panel steps aside for the dialog or sheet it opened, so only one thing is ever on top of the page.
  function hand<T>(value: T | undefined, open: (value: T) => void) {
    setViewing(null);
    if (value) open(value);
  }

  return (
    <>
      <FilterBar
        searchLabel="Search registrations"
        search={name}
        onSearch={setName}
        resetKey={resetKey}
        active={status ? 1 : 0}
        filtered={filtered}
        onClear={clear}
      >
        <FilterSelect
          label="Status"
          options={statusOptions(phase)}
          value={status}
          onChange={setStatus}
        />
      </FilterBar>
      <ListBody
        list={rosterList(filterGuests(guests, name, status), state)}
        icon="user"
        noun={["registration", "registrations"]}
        emptyText="People who register for this event will appear here."
        filtered={filtered}
        noMatchText="No one signed up matches that."
        onClear={clear}
        order="Sign-up order."
      >
        {(rows) => (
          <PersonList
            label="Registrations"
            rows={rows}
            rowKey={(g) => g.registration.id}
            person={(g) => ({
              name: g.registration.memberName,
              onOpen: () => setViewing(g),
              plain: true,
            })}
            columns={columns(phase)}
          />
        )}
      </ListBody>
      {viewing && (
        <EventPersonPanel
          person={guestPerson(viewing)}
          eventName={eventName}
          phase={phase}
          onClose={() => setViewing(null)}
          onEditTime={() => hand(viewing.checkIn, setEditing)}
          onRemoveRegistration={() => hand(viewing.registration, setRemoving)}
          onRemoveCheckIn={() => hand(viewing.checkIn, setRemovingCheckIn)}
        />
      )}
      {editing && (
        <EditCheckInTimeSheet
          checkIn={editing}
          eventName={eventName}
          onClose={() => setEditing(null)}
        />
      )}
      {removing && (
        <RemoveRegistrationDialog
          record={removing}
          onClose={() => setRemoving(null)}
        />
      )}
      {removingCheckIn && (
        <RemoveCheckInDialog
          record={removingCheckIn}
          onClose={() => setRemovingCheckIn(null)}
        />
      )}
    </>
  );
}
