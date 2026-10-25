import { ActionMenu, Button } from "@/ui";
import type { Organization } from "./api";
import styles from "./OrganizationHeader.module.css";

type Props = {
  organization: Organization;
  onEdit: () => void;
  onDelete: () => void;
  // Opens the "Add member" sheet.
  onAdd?: () => void;
};

// The top of an organization's page: its name, the one main action and a ⋯ menu for the rest.
export function OrganizationHeader({
  organization: o,
  onEdit,
  onDelete,
  onAdd,
}: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.who}>
        <h1 className={styles.name}>{o.name}</h1>
      </div>
      <div className={styles.actions}>
        {onAdd && <Button onClick={onAdd}>Add member</Button>}
        <ActionMenu
          label={`More actions for ${o.name}`}
          title={o.name}
          items={[
            { label: "Edit", onSelect: onEdit },
            { label: "Delete", danger: true, onSelect: onDelete },
          ]}
        />
      </div>
    </header>
  );
}
