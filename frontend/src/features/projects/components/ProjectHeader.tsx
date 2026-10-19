import { Button, Chip, Icon } from "@/ui";
import type { Project } from "../api";
import { statusLabel, statusTone } from "../labels";
import styles from "./ProjectHeader.module.css";

type Props = {
  project: Project;
  onEdit: () => void;
  onDelete: () => void;
  onRecord: () => void;
};

// The top of a project's page: its name and status, what it is for, and what can be done to it. A long name wraps.
export function ProjectHeader({
  project: p,
  onEdit,
  onDelete,
  onRecord,
}: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.main}>
        <div className={styles.title}>
          <h1 className={styles.name}>{p.name}</h1>
          <Chip tone={statusTone[p.status]}>{statusLabel[p.status]}</Chip>
        </div>
        {p.description && <p className={styles.text}>{p.description}</p>}
      </div>
      <div className={styles.actions}>
        <Button variant="secondary" onClick={onEdit}>
          Edit
        </Button>
        <Button variant="secondary" onClick={onDelete}>
          Delete
        </Button>
        <Button onClick={onRecord}>
          <Icon name="plus" size={16} />
          Record contribution
        </Button>
      </div>
    </header>
  );
}
