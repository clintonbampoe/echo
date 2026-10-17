import { formatTime } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { Chip, ListBody, PersonList, type PersonColumn } from "@/ui";
import type { CheckIn } from "../api";
import { personHref } from "../view";

type Props = {
  list: Paged<CheckIn>;
  filtered: boolean;
  onClear: () => void;
  onEdit: (checkIn: CheckIn) => void;
  onRemove: (checkIn: CheckIn) => void;
};

const columns: PersonColumn<CheckIn>[] = [
  {
    header: "Kind",
    width: "110px",
    chip: true,
    cell: (c) => (
      <Chip tone={c.personKind === "Member" ? "neutral" : "warning"}>
        {c.personKind}
      </Chip>
    ),
  },
  {
    header: "Checked in",
    width: "120px",
    cell: (c) => formatTime(c.checkedInAt),
  },
];

// Who is marked present at a session, newest first. Each row opens the person and has a menu to fix or undo it.
export function CheckInList({
  list,
  filtered,
  onClear,
  onEdit,
  onRemove,
}: Props) {
  return (
    <ListBody
      list={list}
      icon="calendar-check"
      noun={["check-in", "check-ins"]}
      emptyTitle="No one is marked present yet"
      emptyText="Use the search on the right as people arrive. Members and visitors both work."
      noMatchText="Try a different name, or clear the filters."
      filtered={filtered}
      onClear={onClear}
      order="Newest first."
    >
      {(rows) => (
        <PersonList
          label="Check-ins"
          rows={rows}
          rowKey={(c) => c.id}
          person={(c) => ({
            name: c.personName,
            href: personHref(c),
            plain: true,
          })}
          columns={columns}
          actions={(c) => [
            { label: "Edit check-in time", onSelect: () => onEdit(c) },
            {
              label: "Remove from session",
              danger: true,
              separated: true,
              onSelect: () => onRemove(c),
            },
          ]}
        />
      )}
    </ListBody>
  );
}
