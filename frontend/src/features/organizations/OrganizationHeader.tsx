import { Button } from "@/ui";
import type { Organization } from "./api";
import styles from "./OrganizationHeader.module.css";
import { OrganizationTile } from "./OrganizationTile";

type Props = {
  organization: Organization;
  onEdit: () => void;
  onDelete: () => void;
  // Opens the "Add member" sheet.
  onAdd?: () => void;
};

// The top of an organization's page: its tile, name and description, and what can be done to the group.
export function OrganizationHeader({
  organization: o,
  onEdit,
  onDelete,
  onAdd,
}: Props) {
  return (
    <header className={styles.header}>
      <OrganizationTile name={o.name} size={56} />
      <div className={styles.who}>
        <h1 className={styles.name}>{o.name}</h1>
        {o.description && <p className={styles.text}>{o.description}</p>}
      </div>
      <div className={styles.actions}>
        {onAdd && (
          <Button className={styles.add} onClick={onAdd}>
            Add member
          </Button>
        )}
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
