import { todayText } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, RecordTable, Section } from "@/ui";
import { usePersonCheckIns, usePersonRegistrations } from "../../hooks/queries";
import { statusLabel, statusTone } from "../../model/labels";
import { eventStatus } from "../../model/display";

// The events a person signed up for, and whether they came. A row opens the event.
export function RegisteredEvents({ personId }: { personId: string }) {
  const today = todayText();
  const attended = new Set(
    usePersonCheckIns(personId).rows.map((c) => c.eventId),
  );
  return (
    <Section
      title="Registered for"
      hint="Events they signed up for, and whether they came."
    >
      <RecordTable
        label="Event registrations"
        title="Event registrations"
        list={usePersonRegistrations(personId)}
        rowKey={(r) => r.id}
        href={(r) => paths.event(r.eventId)}
        scroll
        empty={{
          icon: "calendar",
          title: "No event sign-ups",
          text: "Events they register for are listed here.",
        }}
        noun={["registration", "registrations"]}
        columns={[
          { header: "Event", cell: (r) => r.eventName },
          { header: "Date", cell: (r) => formatDate(r.eventStartDate) },
          {
            header: "Status",
            cell: (r) => {
              const status = eventStatus(
                r.eventStartDate,
                attended.has(r.eventId),
                today,
              );
              return (
                <Chip tone={statusTone[status]}>{statusLabel[status]}</Chip>
              );
            },
          },
        ]}
      />
    </Section>
  );
}
