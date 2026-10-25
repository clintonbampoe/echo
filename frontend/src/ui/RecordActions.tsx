import { Button } from "./Button";
import styles from "./RecordActions.module.css";

type Props = {
  // The main step, full width. Left out for a record nothing can change on.
  edit?: { label: string; onClick: () => void };
  // What removes it, as red words under the main step.
  remove: { label: string; onClick: () => void }[];
};

// The foot of a record's panel: Edit as the one big button, and what deletes it as plain red text beneath.
export function RecordActions({ edit, remove }: Props) {
  return (
    <div className={styles.actions}>
      {edit && (
        <Button block onClick={edit.onClick}>
          {edit.label}
        </Button>
      )}
      {remove.map((r) => (
        <button
          key={r.label}
          type="button"
          className={styles.remove}
          onClick={r.onClick}
        >
          {r.label}
        </button>
      ))}
    </div>
  );
}
