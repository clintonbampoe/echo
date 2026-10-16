import { Button, Chip } from "@/ui";
import type { Session } from "../api";
import { sessionTitle, shortDay, statusTone, timeRange } from "../day";
import styles from "./SessionHeader.module.css";

type Props = {
  session: Session;
  onEdit: () => void;
  onDelete: () => void;
  onComplete: () => void;
  onCancel: () => void;
};

// Who this session is, when it was, and what can be done to it. Only a session still ahead can be completed or cancelled.
export function SessionHeader({
  session: s,
  onEdit,
  onDelete,
  onComplete,
  onCancel,
}: Props) {
  const when = [`${shortDay(s.date)} ${s.date.slice(0, 4)}`, timeRange(s)]
    .filter(Boolean)
    .join(", ");
  return (
    <header className={styles.header}>
      <div>
        <h1 className={styles.title}>
          {sessionTitle(s)}
          <Chip tone="neutral">{s.category}</Chip>
          <Chip tone={statusTone[s.status]}>{s.status}</Chip>
        </h1>
        <p className={styles.meta}>
          {when}.{s.topic ? ` Topic: ${s.topic}.` : ""}
        </p>
        {(s.notes || s.createdByName) && (
          <p className={styles.notes}>
            {s.notes}
            {s.notes && s.createdByName ? " " : ""}
            {s.createdByName && `Created by ${s.createdByName}.`}
          </p>
        )}
      </div>
      <div className={styles.buttons}>
        {s.status === "Scheduled" && (
          <Button onClick={onComplete}>Mark completed</Button>
        )}
        <Button variant="secondary" onClick={onEdit}>
          Edit
        </Button>
        {s.status === "Scheduled" && (
          <Button variant="secondary" onClick={onCancel}>
            Cancel session
          </Button>
        )}
        <Button
          variant="secondary"
          className={styles.danger}
          onClick={onDelete}
        >
          Delete
        </Button>
      </div>
    </header>
  );
}
