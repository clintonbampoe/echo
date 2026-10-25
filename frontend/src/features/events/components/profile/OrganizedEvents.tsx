import { formatDate, formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { RecordTable, Section } from "@/ui";
import { useOrganizedEvents } from "../../hooks/queries";

// The events a person organizes, with how many have registered. A row opens the event.
export function OrganizedEvents({ personId }: { personId: string }) {
  return (
    <Section
      title="Events organized"
      hint="Events this person organizes, with registrations."
    >
      <RecordTable
        label="Events organized"
        title="Events organized"
        list={useOrganizedEvents(personId)}
        rowKey={(e) => e.id}
        href={(e) => paths.event(e.id)}
        scroll
        empty={{
          icon: "calendar",
          title: "Not organizing an event",
          text: "Events they are the organizer of are listed here with who has registered.",
        }}
        noun={["event", "events"]}
        columns={[
          { header: "Event", cell: (e) => e.name },
          { header: "Date", cell: (e) => formatDate(e.startDate) },
          {
            header: "Registered",
            cell: (e) => formatNumber(e.registrationCount),
            end: true,
          },
        ]}
      />
    </Section>
  );
}
