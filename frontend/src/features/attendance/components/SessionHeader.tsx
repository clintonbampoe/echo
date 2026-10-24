import { ActionMenu, Button, Chip } from "@/ui";
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

// Who this session is, when it was, and what can be done to it: the main step (complete it) and a ⋯ menu for the
// rest. Only a session still ahead can be completed or cancelled.
export function SessionHeader({
  session: s,
  onEdit,
  onDelete,
  onComplete,
  onCancel,
}: Props) {
  const scheduled = s.status === "Scheduled";
  const line = [shortDay(s.date), timeRange(s), s.topic]
    .filter(Boolean)
    .join(" · ");
  const items = [
    { label: "Edit", onSelect: onEdit },
    ...(scheduled ? [{ label: "Cancel session", onSelect: onCancel }] : []),
    { label: "Delete", danger: true, onSelect: onDelete },
  ];
  return (
    <header className={styles.header}>
      <div className={styles.who}>
        <h1 className={styles.title}>
          {sessionTitle(s)}
          {s.status !== "Completed" && (
            <Chip tone={statusTone[s.status]}>{s.status}</Chip>
          )}
        </h1>
        <p className={styles.meta}>{line}</p>
        {(s.notes || s.createdByName) && (
          <p className={styles.notes}>
            {s.notes}
            {s.notes && s.createdByName ? " " : ""}
            {s.createdByName && `Created by ${s.createdByName}.`}
          </p>
        )}
      </div>
      <div className={styles.buttons}>
        {scheduled && <Button onClick={onComplete}>Mark completed</Button>}
        <ActionMenu
          label={`More actions for ${sessionTitle(s)}`}
          title={sessionTitle(s)}
          items={items}
        />
      </div>
    </header>
  );
}
