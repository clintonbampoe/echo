import { ActionMenu, Button } from "@/ui";
import type { Project } from "../api";
import styles from "./ProjectHeader.module.css";

type Props = {
  project: Project;
  onEdit: () => void;
  onDelete: () => void;
  onRecord: () => void;
};

// The top of a project's page: its name, the category and manager, the one main action and a ⋯ menu for the rest.
export function ProjectHeader({
  project: p,
  onEdit,
  onDelete,
  onRecord,
}: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.main}>
        <h1 className={styles.name}>{p.name}</h1>
        <p className={styles.line}>
          {p.categoryName} · Manager{" "}
          <span className={styles.manager}>{p.managerName}</span>
        </p>
        {p.description && <p className={styles.text}>{p.description}</p>}
      </div>
      <div className={styles.actions}>
        <Button onClick={onRecord}>Add contribution</Button>
        <ActionMenu
          label={`More actions for ${p.name}`}
          title={p.name}
          items={[
            { label: "Edit", onSelect: onEdit },
            { label: "Delete", danger: true, onSelect: onDelete },
          ]}
        />
      </div>
    </header>
  );
}
