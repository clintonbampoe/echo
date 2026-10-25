import { formatNumber } from "@/lib/format";
import { DetailCard, DetailGroup } from "@/ui";
import type { EventItem } from "../../api/events";
import { eventDates } from "../../model/headline";

// What an event is: who hosts it and who is in charge, when and where, how many it takes, and what it is about.
export function EventDetails({ event: e }: { event: EventItem }) {
  return (
    <DetailCard label="Event details">
      <DetailGroup
        title="Event"
        rows={[
          ["Group", e.organizationName],
          ["Organizer", e.organizerName],
          ["When", eventDates(e)],
          ["Place", e.location],
          ["Capacity", e.capacity ? formatNumber(e.capacity) : "No limit"],
          ["About", e.description],
        ]}
      />
    </DetailCard>
  );
}
