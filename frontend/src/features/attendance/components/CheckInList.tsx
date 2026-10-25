import { formatTime } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { ListBody, PersonList, type PersonColumn } from "@/ui";
import type { CheckIn } from "../api";

type Props = {
  list: Paged<CheckIn>;
  filtered: boolean;
  onClear: () => void;
  onOpen: (checkIn: CheckIn) => void;
  onEdit: (checkIn: CheckIn) => void;
  onRemove: (checkIn: CheckIn) => void;
};

const columns: PersonColumn<CheckIn>[] = [
  {
    header: "Checked in",
    width: "120px",
    end: true,
    keep: true,
    cell: (c) => formatTime(c.checkedInAt),
  },
];

// Who is marked present at a session, newest first. A row opens their record, where it can be changed or removed.
export function CheckInList({
  list,
  filtered,
  onClear,
  onOpen,
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
      footerInside
    >
      {(rows, footer) => (
        <PersonList
          label="Check-ins"
          title="Check-ins"
          rows={rows}
          rowKey={(c) => c.id}
          person={(c) => ({
            name: c.personName,
            onOpen: () => onOpen(c),
            detail: c.personKind,
          })}
          columns={columns}
          footer={footer}
          actions={(c) => [
            { label: "Edit time", onSelect: () => onEdit(c) },
            {
              label: "Remove from session",
              danger: true,
              onSelect: () => onRemove(c),
            },
          ]}
          compact
          flat
        />
      )}
    </ListBody>
  );
}
