import { formatClock, formatDayMonth } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Button, Chip, DetailGroup, Sheet } from "@/ui";
import {
  missingText,
  panelTitle,
  signedUpText,
  type EventPerson,
} from "../eventPerson";
import type { Phase } from "../roster";
import styles from "./EventPersonPanel.module.css";
import { PanelIdentity } from "@/ui/PanelIdentity";

type Props = {
  person: EventPerson;
  eventName: string;
  phase: Phase;
  onClose: () => void;
  // Each one closes the panel and opens its own dialog or sheet on the page behind.
  onEditTime: () => void;
  onRemoveRegistration: () => void;
  onRemoveCheckIn: () => void;
};

// A person shown on an event, opened from either roster tab. Everything here is about this event;
// the person's own details are on their member page, one deliberate link away (design rule S6).
export function EventPersonPanel({
  person,
  eventName,
  phase,
  onClose,
  onEditTime,
  onRemoveRegistration,
  onRemoveCheckIn,
}: Props) {
  const { registration, checkIn } = person;
  const footer = (
    <div className={styles.footer}>
      <Button variant="secondary" onClick={onClose}>
        Close
      </Button>
      {checkIn && (
        <Button variant="secondary" onClick={onEditTime}>
          Edit check-in time
        </Button>
      )}
    </div>
  );

  return (
    <Sheet
      open
      onClose={onClose}
      title={panelTitle(person)}
      subtitle={`At ${eventName}`}
      footer={footer}
    >
      <div className={styles.body}>
        <PanelIdentity
          name={person.name}
          meta={
            checkIn ? (
              <Chip tone="positive">Checked in</Chip>
            ) : (
              <Chip tone={phase === "over" ? "warning" : "neutral"}>
                {missingText(phase)}
              </Chip>
            )
          }
          profileHref={paths.member(person.memberId, "events")}
        />
        <DetailGroup
          title="Registration"
          rows={[
            [
              "Signed up",
              signedUpText(
                person,
                (day) => `Registered ${formatDayMonth(day)}`,
              ),
            ],
          ]}
        />
        <DetailGroup
          title="Check-in"
          rows={[
            [
              "Arrived",
              checkIn ? formatClock(checkIn.checkInTime) : missingText(phase),
            ],
          ]}
        />
        <div className={styles.remove}>
          <p className={styles.removeText}>
            Removing a record here keeps them as a church member.
          </p>
          <div className={styles.removeActions}>
            {registration && (
              <Button variant="secondary" onClick={onRemoveRegistration}>
                Remove registration
              </Button>
            )}
            {checkIn && (
              <Button variant="secondary" onClick={onRemoveCheckIn}>
                Remove check-in
              </Button>
            )}
          </div>
        </div>
      </div>
    </Sheet>
  );
}
