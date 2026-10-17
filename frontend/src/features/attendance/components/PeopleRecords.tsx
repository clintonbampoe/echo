import { formatDate, formatTime } from "@/lib/format";
import { Chip, ListBody, PersonList, type PersonColumn } from "@/ui";
import type { CheckIn, RecordsQuery } from "../api";
import { sessionTitle } from "../day";
import { useRecordCheckIns } from "../hooks";
import { personHref } from "../view";

type Props = { query: RecordsQuery; narrowed: boolean; onClear: () => void };

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
    header: "Session",
    width: "minmax(140px, 1.4fr)",
    cell: (c) => sessionTitle(c),
  },
  {
    header: "Date",
    width: "minmax(110px, 0.9fr)",
    cell: (c) => formatDate(c.sessionDate),
  },
  {
    header: "Checked in",
    width: "110px",
    cell: (c) => formatTime(c.checkedInAt),
  },
];

// Everyone who was marked present in the period, one row for each time they came, latest session first. A row opens the person.
export function PeopleRecords({ query, narrowed, onClear }: Props) {
  const list = useRecordCheckIns(query);
  return (
    <ListBody
      list={list}
      icon="calendar-check"
      noun={["check-in", "check-ins"]}
      emptyTitle="No one was marked present in this period"
      emptyText="People marked present at a session in the days you chose will be listed here. Try a longer period."
      noMatchText="Try a different name, another kind or type, or a longer period."
      filtered={narrowed}
      onClear={onClear}
      order="Latest session first."
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
        />
      )}
    </ListBody>
  );
}
