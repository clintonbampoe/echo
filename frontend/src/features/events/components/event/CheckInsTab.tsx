import { useState } from "react";
import { formatClock } from "@/lib/format";
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
import {
  checkInFilters,
  toArrival,
  type Arrival,
  type Phase,
} from "../../model/roster";
import { EditCheckInTimeSheet } from "../form/EditCheckInTimeSheet";
import { arrivalPerson } from "../../model/person";
import { EventPersonPanel } from "./EventPersonPanel";
import { RemoveCheckInDialog } from "../dialogs/RemoveDialogs";
import { useEventCheckIns } from "../../hooks/queries";

type Props = {
  eventId: string;
  eventName: string;
  phase: Phase;
};

const options = [
  { value: "", label: "Type" },
  { value: "registered", label: "Registered" },
  { value: "walk-in", label: "Walk-in" },
];

const columns: PersonColumn<Arrival>[] = [
  {
    header: "Checked in",
    width: "minmax(120px, 1fr)",
    cell: (a) => formatClock(a.checkIn.checkInTime),
  },
  {
    header: "Type",
    width: "minmax(120px, 1fr)",
    chip: true,
    cell: (a) => (
      <Chip tone={a.walkIn ? "warning" : "neutral"}>
        {a.walkIn ? "Walk-in" : "Registered"}
      </Chip>
    ),
  },
];

type ToolbarProps = {
  name: string;
  onName: (name: string) => void;
  kind: string;
  onKind: (kind: string) => void;
  resetKey: number;
  filtered: boolean;
  onClear: () => void;
};

// The search box and the type choice above the list.
function Toolbar({
  name,
  onName,
  kind,
  onKind,
  resetKey,
  filtered,
  onClear,
}: ToolbarProps) {
  return (
    <FilterBar
      searchLabel="Search check-ins"
      search={name}
      onSearch={onName}
      resetKey={resetKey}
      active={kind ? 1 : 0}
      filtered={filtered}
      onClear={onClear}
    >
      <FilterSelect
        label="Type"
        options={options}
        value={kind}
        onChange={onKind}
      />
    </FilterBar>
  );
}

// Who came to the event, in the order they arrived, a page at a time. The server searches by name and narrows by type
// (signed up, or a walk-in), and each row says which it is, so the tab reads only what it shows, and only once it is
// open. A row opens that person's panel for this event (design rule S6).
export function CheckInsTab({ eventId, eventName, phase }: Props) {
  const [name, setName] = useState("");
  const [kind, setKind] = useState("");
  const list = useEventCheckIns(eventId, checkInFilters(name, kind));
  const arrivals = { ...list, rows: list.rows.map(toArrival) };
  const [resetKey, setResetKey] = useState(0);
  const [viewing, setViewing] = useState<Arrival | null>(null);
  const [editing, setEditing] = useState<EventCheckIn | null>(null);
  const [removing, setRemoving] = useState<EventCheckIn | null>(null);
  const filtered = name !== "" || kind !== "";

  function clear() {
    setName("");
    setKind("");
    setResetKey(resetKey + 1);
  }

  // The panel steps aside for the dialog or sheet it opened, so only one thing is ever on top of the page.
  function hand(open: (value: EventCheckIn) => void) {
    if (!viewing) return;
    setViewing(null);
    open(viewing.checkIn);
  }

  return (
    <>
      <ListCard
        title="Check-ins"
        toolbar={
          <Toolbar
            name={name}
            onName={setName}
            kind={kind}
            onKind={setKind}
            resetKey={resetKey}
            filtered={filtered}
            onClear={clear}
          />
        }
      >
        <ListBody
          list={arrivals}
          icon="calendar-check"
          noun={["check-in", "check-ins"]}
          emptyText="People who come to this event will appear here."
          filtered={filtered}
          noMatchText="No one checked in matches that."
          onClear={clear}
          order="Arrival order."
          footerInside
        >
          {(rows, footer) => (
            <PersonList
              label="Check-ins"
              title="Check-ins"
              rows={rows}
              rowKey={(a) => a.checkIn.id}
              person={(a) => ({
                name: a.checkIn.memberName,
                onOpen: () => setViewing(a),
                plain: true,
              })}
              columns={columns}
              footer={footer}
              compact
              flat
            />
          )}
        </ListBody>
      </ListCard>
      {viewing && (
        <EventPersonPanel
          person={arrivalPerson(viewing)}
          eventName={eventName}
          phase={phase}
          onClose={() => setViewing(null)}
          onEditTime={() => hand(setEditing)}
          onRemoveRegistration={() => setViewing(null)}
          onRemoveCheckIn={() => hand(setRemoving)}
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
        <RemoveCheckInDialog
          record={removing}
          onClose={() => setRemoving(null)}
        />
      )}
    </>
  );
}
