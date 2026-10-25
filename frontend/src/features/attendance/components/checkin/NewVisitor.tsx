import { Button } from "@/ui";
import { splitName } from "../../model/matches";
import styles from "./MatchList.module.css";

type Props = {
  text: string;
  busy: boolean;
  onAdd: (name: { firstName: string; lastName: string }) => void;
};

// A visitor nobody has met before: add them by the name typed, and they are marked present straight away.
export function NewVisitor({ text, busy, onAdd }: Props) {
  const name = splitName(text);
  return (
    <div>
      <p className={styles.note}>No visitor named &ldquo;{text}&rdquo;.</p>
      {name ? (
        <>
          <p className={styles.note}>
            Add them as a new visitor and they will be marked present straight
            away.
          </p>
          <Button size="sm" disabled={busy} onClick={() => onAdd(name)}>
            Add as new visitor
          </Button>
        </>
      ) : (
        <p className={styles.note}>
          Type the first and last name to add a new visitor.
        </p>
      )}
    </div>
  );
}
