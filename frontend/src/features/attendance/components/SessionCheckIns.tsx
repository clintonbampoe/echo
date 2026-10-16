import { useState } from "react";
import { FilterBar, FilterSelect } from "@/ui";
import type { CheckIn, PersonKind, Session } from "../api";
import { useCheckIns } from "../hooks";
import { CheckInList } from "./CheckInList";
import { EditCheckInTimeSheet, RemoveCheckInDialog } from "./CheckInDialogs";

const kinds = [
  { value: "", label: "Kind" },
  { value: "Member", label: "Members" },
  { value: "Visitor", label: "Visitors" },
];

// Who is marked present at one session: the search and Kind filter, the list, and fixing or removing a check-in.
export function SessionCheckIns({ session }: { session: Session }) {
  const [name, setName] = useState("");
  const [kind, setKind] = useState("");
  const [resetKey, setResetKey] = useState(0);
  const [editing, setEditing] = useState<CheckIn | null>(null);
  const [removing, setRemoving] = useState<CheckIn | null>(null);
  const list = useCheckIns(session.id, {
    name: name || undefined,
    kind: (kind || undefined) as PersonKind | undefined,
  });
  const filtered = name !== "" || kind !== "";

  function clear() {
    setName("");
    setKind("");
    setResetKey(resetKey + 1);
  }

  return (
    <>
      <FilterBar
        searchLabel="Search check-ins"
        search={name}
        onSearch={setName}
        resetKey={resetKey}
        active={kind ? 1 : 0}
        filtered={filtered}
        onClear={clear}
      >
        <FilterSelect
          label="Kind"
          options={kinds}
          value={kind}
          onChange={setKind}
        />
      </FilterBar>
      <CheckInList
        list={list}
        filtered={filtered}
        onClear={clear}
        onEdit={setEditing}
        onRemove={setRemoving}
      />
      {editing && (
        <EditCheckInTimeSheet
          checkIn={editing}
          session={session}
          onClose={() => setEditing(null)}
        />
      )}
      {removing && (
        <RemoveCheckInDialog
          checkIn={removing}
          onClose={() => setRemoving(null)}
        />
      )}
    </>
  );
}
