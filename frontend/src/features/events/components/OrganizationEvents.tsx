import { formatDayTime } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { RecordTable, Section } from "@/ui";
import type { HostedEvent } from "../api";

// The events an organization is hosting. Events are created on the Events page, never from here. A row
// opens the event.
export function OrganizationEvents({ list }: { list: Paged<HostedEvent> }) {
  return (
    <Section hint="Events this group is hosting.">
      <RecordTable
        label="Upcoming events"
        list={list}
        rowKey={(e) => e.id}
        href={(e) => paths.event(e.id)}
        empty={{
          icon: "calendar",
          title: "No upcoming events",
          text: "Events this group is hosting appear here. Events are created from the Events page.",
        }}
        noun={["upcoming event", "upcoming events"]}
        columns={[
          { header: "Event", cell: (e) => e.name },
          {
            header: "When",
            cell: (e) => formatDayTime(e.startDate, e.startTime),
          },
          { header: "Location", cell: (e) => e.location || "–" },
          { header: "Organizer", cell: (e) => e.organizerName },
        ]}
      />
    </Section>
  );
}
