import { formatDate, formatTime } from "@/lib/format";
import { PersonRecord, RecordActions, Sheet } from "@/ui";
import type { CheckIn } from "../../api/checkIns";
import { recordTitle } from "../../model/day";
import { personHref } from "../../model/display";

type Props = {
  checkIn: CheckIn;
  // Each one closes the panel and opens its own sheet or dialog on the page behind.
  onEdit: () => void;
  onRemove: () => void;
  onClose: () => void;
};

// One person's attendance at one session. Everything here is about the session; the person's own page is one
// deliberate link away (design rule S6). Changing the time or removing the record is a next step, not this panel.
export function CheckInPersonPanel({
  checkIn: c,
  onEdit,
  onRemove,
  onClose,
}: Props) {
  return (
    <Sheet
      open
      onClose={onClose}
      title="Attendance record"
      footer={
        <RecordActions
          edit={{ label: "Edit", onClick: onEdit }}
          remove={[{ label: "Delete record", onClick: onRemove }]}
        />
      }
    >
      <PersonRecord
        name={c.personName}
        meta={`${c.personKind} · Present`}
        rows={[
          ["Session", recordTitle(c)],
          ["Date", formatDate(c.sessionDate)],
          ["Checked in", formatTime(c.checkedInAt)],
        ]}
        profileHref={personHref(c)}
        profileNote={`${c.personName} · opens the Attendance tab`}
      />
    </Sheet>
  );
}
