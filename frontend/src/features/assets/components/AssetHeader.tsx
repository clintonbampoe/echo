import { ActionMenu, Button, Chip } from "@/ui";
import type { Asset } from "../api";
import { statusLabel, statusTone } from "../labels";
import styles from "./AssetHeader.module.css";

type Props = { asset: Asset; onEdit: () => void; onDelete: () => void };

// The top of an asset's page: its name and status, the category it is filed under, Edit, and a ⋯ menu for Delete.
export function AssetHeader({ asset: a, onEdit, onDelete }: Props) {
  return (
    <header className={styles.header}>
      <div className={styles.main}>
        <div className={styles.title}>
          <h1 className={styles.name}>{a.name}</h1>
          <Chip tone={statusTone[a.status]}>{statusLabel[a.status]}</Chip>
        </div>
        <p className={styles.text}>{a.categoryName}</p>
      </div>
      <div className={styles.actions}>
        <Button onClick={onEdit}>Edit</Button>
        <ActionMenu
          label={`More actions for ${a.name}`}
          title={a.name}
          items={[{ label: "Delete", danger: true, onSelect: onDelete }]}
        />
      </div>
    </header>
  );
}
