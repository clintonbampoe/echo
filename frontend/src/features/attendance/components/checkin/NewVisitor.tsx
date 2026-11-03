import { useState } from "react";
import { genderChoices, type Gender } from "@/features/members";
import { Button, SegmentedField } from "@/ui";
import { splitName } from "../../model/matches";
import type { NewName } from "../../hooks/marking";
import styles from "./MatchList.module.css";

type Props = {
  text: string;
  busy: boolean;
  onAdd: (name: NewName) => void;
};

// A visitor nobody has met before: add them by the name typed and a gender, and they are marked present straight away.
export function NewVisitor({ text, busy, onAdd }: Props) {
  const name = splitName(text);
  const [gender, setGender] = useState<Gender | "">("");
  return (
    <div>
      <p className={styles.note}>No visitor named &ldquo;{text}&rdquo;.</p>
      {name ? (
        <>
          <p className={styles.note}>
            Add them as a new visitor and they will be marked present straight
            away.
          </p>
          <SegmentedField
            label="Gender"
            options={genderChoices}
            value={gender}
            onChange={setGender}
            joined
          />
          <Button
            size="sm"
            disabled={busy || gender === ""}
            onClick={() => gender && onAdd({ ...name, gender })}
          >
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
