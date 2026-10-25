import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { Alert, Notice, SearchField, SegmentedField } from "@/ui";
import styles from "./RecordPanel.module.css";
import { RecordResults } from "./RecordResults";
import { useRecorder } from "./useRecorder";
import { isFull, logText, type Mode } from "./record";
import type { EventItem } from "./api";

type Props = {
  event: EventItem;
  // Which of the two to start on.
  initial: Mode;
  // Who has signed up and who has come already, to mark them in the search.
  registered: ReadonlySet<string>;
  checkedIn: ReadonlySet<string>;
};

const modes = [
  { value: "register", label: "Register" },
  { value: "check-in", label: "Check in" },
] as const;

// Sign people up, or check them in at the door: search for a member, then tap. Each one is saved as it is tapped.
// When every spot is taken, registering stops (with the way to raise the capacity), and checking in carries on.
export function RecordPanel({ event, initial, registered, checkedIn }: Props) {
  const [mode, setMode] = useState<Mode>(initial);
  const [text, setText] = useState("");
  const { done, busy, problem, record } = useRecorder(event.id);
  const closed = isFull(event) && mode === "register";

  return (
    <section aria-label="Register or check in" className={styles.panel}>
      <SegmentedField
        label="What are you recording"
        options={modes}
        value={mode}
        onChange={setMode}
      />
      {closed && (
        <Notice tone="warning" title="Registration is closed.">
          All {formatNumber(event.capacity ?? 0)} spots are taken. To register
          more people, raise the capacity in Edit.
        </Notice>
      )}
      <div>
        <SearchField
          label="Search members"
          placeholder="Search by name or phone"
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <RecordResults
          mode={mode}
          text={text.trim()}
          closed={closed}
          existing={mode === "register" ? registered : checkedIn}
          done={done[mode]}
          busy={busy}
          onRecord={(member) => void record(mode, member)}
        />
      </div>
      {problem && <Alert title={problem.title}>{problem.message}</Alert>}
      <p role="status" className={styles.log}>
        {logText(mode, done[mode].length)}
      </p>
    </section>
  );
}
