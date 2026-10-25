import { useState } from "react";
import { FilterBar, FilterSelect, ListCard } from "@/ui";
import type { PersonKind, Session } from "../api";
import { useCheckIns } from "../hooks";
import { CheckInList } from "./CheckInList";
import { useCheckInPanels } from "./useCheckInPanels";

const kinds = [
  { value: "", label: "Kind" },
  { value: "Member", label: "Members" },
  { value: "Visitor", label: "Visitors" },
];

// Who is marked present at one session: the search and Kind filter, the list, and the panels a row opens.
export function SessionCheckIns({ session }: { session: Session }) {
  const [name, setName] = useState("");
  const [kind, setKind] = useState("");
  const [resetKey, setResetKey] = useState(0);
  const { open, edit, remove, panels } = useCheckInPanels();
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
      <ListCard
        title="Check-ins"
        toolbar={
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
        }
      >
        <CheckInList
          list={list}
          filtered={filtered}
          onClear={clear}
          onOpen={open}
          onEdit={edit}
          onRemove={remove}
        />
      </ListCard>
      {panels}
    </>
  );
}
