import { useState } from "react";
import { formatClock, formatDayTime } from "@/lib/format";
import {
  Chip,
  FilterBar,
  FilterSelect,
  ListBody,
  PersonList,
  type PersonColumn,
  ListCard,
} from "@/ui";
import type { EventCheckIn } from "../../api/checkIns";
import type { EventRegistration } from "../../api/registrations";
import {
  guestStatus,
  registrationFilters,
  toGuest,
  type Guest,
  type Phase,
} from "../../model/roster";
import { EditCheckInTimeSheet } from "../form/EditCheckInTimeSheet";
import { guestPerson } from "../../model/person";
import { EventPersonPanel } from "./EventPersonPanel";
import { useEventRegistrations } from "../../hooks/queries";
import {
  RemoveCheckInDialog,
  RemoveRegistrationDialog,
} from "../dialogs/RemoveDialogs";

type Props = {
  eventId: string;
  eventName: string;
  phase: Phase;
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

// Who signed up for the event, and whether they came, a page at a time. The server searches by name and narrows by
// status, and each row says whether the person came, so the tab reads only what it shows, and only once it is open.
// A row opens that person's panel for this event (design rule S6); every action lives in there.
export function RegistrationsTab({ eventId, eventName, phase }: Props) {
  const [name, setName] = useState("");
  const [status, setStatus] = useState("");
  const list = useEventRegistrations(
    eventId,
    registrationFilters(name, status),
  );
  const guests = { ...list, rows: list.rows.map(toGuest) };
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
      <ListCard
        title="Registrations"
        toolbar={
          <Toolbar
            name={name}
            onName={setName}
            status={status}
            onStatus={setStatus}
            phase={phase}
            resetKey={resetKey}
            filtered={filtered}
            onClear={clear}
          />
        }
      >
        <ListBody
          list={guests}
          icon="user"
          noun={["registration", "registrations"]}
          emptyText="People who register for this event will appear here."
          filtered={filtered}
          noMatchText="No one signed up matches that."
          onClear={clear}
          order="Sign-up order."
          footerInside
        >
          {(rows, footer) => (
            <PersonList
              label="Registrations"
              title="Registrations"
              rows={rows}
              rowKey={(g) => g.registration.id}
              person={(g) => ({
                name: g.registration.memberName,
                onOpen: () => setViewing(g),
                plain: true,
              })}
              columns={columns(phase)}
              footer={footer}
              compact
              flat
            />
          )}
        </ListBody>
      </ListCard>
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
      <Pending
        eventName={eventName}
        editing={editing}
        removing={removing}
        removingCheckIn={removingCheckIn}
        onDone={() => {
          setEditing(null);
          setRemoving(null);
          setRemovingCheckIn(null);
        }}
      />
    </>
  );
}

type ToolbarProps = {
  name: string;
  onName: (name: string) => void;
  status: string;
  onStatus: (status: string) => void;
  phase: Phase;
  resetKey: number;
  filtered: boolean;
  onClear: () => void;
};

// The search box and the status choice above the list.
function Toolbar({
  name,
  onName,
  status,
  onStatus,
  phase,
  resetKey,
  filtered,
  onClear,
}: ToolbarProps) {
  return (
    <FilterBar
      searchLabel="Search registrations"
      search={name}
      onSearch={onName}
      resetKey={resetKey}
      active={status ? 1 : 0}
      filtered={filtered}
      onClear={onClear}
    >
      <FilterSelect
        label="Status"
        options={statusOptions(phase)}
        value={status}
        onChange={onStatus}
      />
    </FilterBar>
  );
}

type PendingProps = {
  eventName: string;
  editing: EventCheckIn | null;
  removing: EventRegistration | null;
  removingCheckIn: EventCheckIn | null;
  onDone: () => void;
};

// The sheet or dialog a person's panel handed over to: change a check-in time, or undo a sign-up or a check-in.
function Pending({
  eventName,
  editing,
  removing,
  removingCheckIn,
  onDone,
}: PendingProps) {
  return (
    <>
      {editing && (
        <EditCheckInTimeSheet
          checkIn={editing}
          eventName={eventName}
          onClose={onDone}
        />
      )}
      {removing && (
        <RemoveRegistrationDialog record={removing} onClose={onDone} />
      )}
      {removingCheckIn && (
        <RemoveCheckInDialog record={removingCheckIn} onClose={onDone} />
      )}
    </>
  );
}
