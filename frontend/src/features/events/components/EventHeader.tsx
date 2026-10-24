import { ActionMenu, Button, Chip } from "@/ui";
import type { EventItem } from "../api";
import { eventHeadline } from "../headline";
import styles from "./EventHeader.module.css";
import { isFull } from "../record";

type Props = { event: EventItem; onEdit: () => void; onDelete: () => void };

// The top of an event's page: its name, group and when and where it is, what it is about, Edit, and a ⋯ menu for
// Delete. "Full" shows once every spot is taken, so it is clear why no one more can be registered.
export function EventHeader({ event: e, onEdit, onDelete }: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.main}>
        <div className={styles.title}>
          <h1 className={styles.name}>{e.name}</h1>
          <Chip>{e.organizationName}</Chip>
          {isFull(e) && <Chip tone="warning">Full</Chip>}
        </div>
        <p className={styles.text}>{eventHeadline(e)}</p>
        {e.description && <p className={styles.text}>{e.description}</p>}
      </div>
      <div className={styles.actions}>
        <Button onClick={onEdit}>Edit</Button>
        <ActionMenu
          label={`More actions for ${e.name}`}
          title={e.name}
          items={[{ label: "Delete", danger: true, onSelect: onDelete }]}
        />
      </div>
    </header>
  );
}
