import { ActionMenu, Button } from "@/ui";
import type { EventItem } from "../api";
import { eventSub } from "../rows";
import styles from "./EventHeader.module.css";

type Props = {
  event: EventItem;
  onCheckIn: () => void;
  onRegister: () => void;
  onEdit: () => void;
  onDelete: () => void;
};

// The top of an event's page: its name, the day and place, Check in (the thing done at the door), and a ⋯ menu for
// the rest: registering someone, editing and deleting. Who hosts it and what it is about are under Details.
export function EventHeader({
  event: e,
  onCheckIn,
  onRegister,
  onEdit,
  onDelete,
}: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.main}>
        <h1 className={styles.name}>{e.name}</h1>
        <p className={styles.text}>{eventSub(e)}</p>
      </div>
      <div className={styles.actions}>
        <Button onClick={onCheckIn}>Check in</Button>
        <ActionMenu
          label={`More actions for ${e.name}`}
          title={e.name}
          items={[
            { label: "Register someone", onSelect: onRegister },
            { label: "Edit", onSelect: onEdit },
            { label: "Delete", danger: true, onSelect: onDelete },
          ]}
        />
      </div>
    </header>
  );
}
