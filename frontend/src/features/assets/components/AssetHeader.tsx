import { Button, Chip } from "@/ui";
import type { Asset } from "../api";
import { statusLabel, statusTone } from "../labels";
import styles from "./AssetHeader.module.css";

type Props = { asset: Asset; onEdit: () => void; onDelete: () => void };

// The top of an asset's page: its name and status, the category it is filed under, and what can be done to it. A long
// name wraps.
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
