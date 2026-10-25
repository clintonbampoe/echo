import { formatClock, formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { RecordTable, Section } from "@/ui";
import { usePersonCheckIns } from "../hooks";

// The events a person was checked in to at the door. A row opens the event.
export function AttendedEvents({ personId }: { personId: string }) {
  return (
    <Section title="Attended" hint="Checked in at the door.">
      <RecordTable
        label="Event attendance"
        title="Event attendance"
        list={usePersonCheckIns(personId)}
        rowKey={(c) => c.id}
        href={(c) => paths.event(c.eventId)}
        scroll
        empty={{
          icon: "check",
          title: "No event check-ins",
          text: "Events they were checked in to at the door are listed here.",
        }}
        noun={["event attended", "events attended"]}
        columns={[
          { header: "Event", cell: (c) => c.eventName },
          { header: "Date", cell: (c) => formatDate(c.eventStartDate) },
          { header: "Checked in", cell: (c) => formatClock(c.checkInTime) },
        ]}
      />
    </Section>
  );
}
