import { formatDate, formatTime } from "@/lib/format";
import { Button, Chip, DetailGroup, Sheet } from "@/ui";
import type { CheckIn } from "../api";
import { dayMonth, recordTitle } from "../day";
import { personHref } from "../view";
import styles from "./CheckInPersonPanel.module.css";
import { PanelIdentity } from "@/ui/PanelIdentity";

type Props = {
  checkIn: CheckIn;
  // Each one closes the panel and opens its own sheet or dialog on the page behind.
  onEdit: () => void;
  onRemove: () => void;
  onClose: () => void;
};

// One person's attendance at one session, read only. Everything here is about the session; the person's own
// page is one deliberate link away (design rule S6). Changing or removing the record is a next step, not this panel.
export function CheckInPersonPanel({
  checkIn: c,
  onEdit,
  onRemove,
  onClose,
}: Props) {
  const footer = (
    <div className={styles.actions}>
      <Button variant="secondary" onClick={onRemove} className={styles.delete}>
        Remove check-in
      </Button>
      <Button onClick={onEdit}>Edit check-in</Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={onClose}
      title="Check-in"
      subtitle={`At ${recordTitle(c)}, ${dayMonth(c.sessionDate)}`}
      footer={footer}
    >
      <div className={styles.body}>
        <PanelIdentity
          name={c.personName}
          meta={
            <Chip tone={c.personKind === "Member" ? "neutral" : "warning"}>
              {c.personKind}
            </Chip>
          }
          profileHref={personHref(c)}
        />
        <DetailGroup
          title="Details"
          rows={[
            ["Session", recordTitle(c)],
            ["Date", formatDate(c.sessionDate)],
            ["Checked in", formatTime(c.checkedInAt)],
            ["Came as", c.personKind],
          ]}
        />
      </div>
    </Sheet>
  );
}
