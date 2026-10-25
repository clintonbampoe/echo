import { ActionMenu, Button, Chip } from "@/ui";
import type { Session } from "../../api/sessions";
import { sessionTitle, shortDay, statusTone, timeRange } from "../../model/day";
import styles from "./SessionHeader.module.css";

type Props = {
  session: Session;
  onMark: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onComplete: () => void;
  onCancel: () => void;
};

// Who this session is, when it was, and what can be done to it: Mark present (the thing done as people arrive) and a ⋯
// menu for the rest. Only a session still ahead can be completed or cancelled.
export function SessionHeader({
  session: s,
  onMark,
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
    ...(scheduled ? [{ label: "Mark completed", onSelect: onComplete }] : []),
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
      </div>
      <div className={styles.buttons}>
        <Button onClick={onMark}>Mark present</Button>
        <ActionMenu
          label={`More actions for ${sessionTitle(s)}`}
          title={sessionTitle(s)}
          items={items}
        />
      </div>
    </header>
  );
}
