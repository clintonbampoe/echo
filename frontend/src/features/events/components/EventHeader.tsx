import { Button, Chip } from "@/ui";
import type { EventItem } from "../api";
import { eventHeadline } from "../headline";
import styles from "./EventHeader.module.css";
import { isFull } from "../record";

type Props = { event: EventItem; onEdit: () => void; onDelete: () => void };

// The top of an event's page: its name and group, when and where it is, what it is about, and what can be done to it.
// "Full" shows once every spot is taken, so it is clear why no one more can be registered.
export function EventHeader({ event: e, onEdit, onDelete }: Props) {
  const full = isFull(e);
  return (
    <header className={styles.header}>
      <h1 className={styles.name}>{e.name}</h1>
      <div className={styles.chips}>
        <Chip>{e.organizationName}</Chip>
        {full && <Chip tone="warning">Full</Chip>}
      </div>
      <p className={styles.when}>{eventHeadline(e)}</p>
      {e.description && <p className={styles.text}>{e.description}</p>}
      <div className={styles.actions}>
        <Button variant="secondary" onClick={onEdit}>
          Edit
        </Button>
        <Button variant="secondary" onClick={onDelete}>
          Delete
        </Button>
      </div>
    </header>
  );
}
