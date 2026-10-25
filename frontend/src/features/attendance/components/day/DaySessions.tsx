import { Alert } from "@/ui";
import type { Session } from "../../api/sessions";
import { nearestSessionDay } from "../../model/day";
import { EmptyDay } from "./EmptyDay";
import { SessionList } from "./SessionCard";
import styles from "./DaySessions.module.css";

type Props = {
  day: string;
  // The sessions of the picked day.
  here: Session[];
  // All sessions around it, to point at the nearest day that has one.
  around: Session[];
  // True once the answer is for this day, not the last window still on screen.
  ready: boolean;
  failed: boolean;
  noTypes: boolean;
  onPick: (day: string) => void;
  onNew: () => void;
};

// What sits under the day bar: the day's sessions, or why there are none.
export function DaySessions({
  day,
  here,
  around,
  ready,
  failed,
  noTypes,
  onPick,
  onNew,
}: Props) {
  return (
    <>
      {failed && (
        <Alert title="We could not load the sessions">
          Check your connection and try again.
        </Alert>
      )}
      {ready && here.length > 0 && <SessionList day={day} sessions={here} />}
      {ready && here.length === 0 && (
        <div className={styles.empty}>
          <EmptyDay
            day={day}
            noTypes={noTypes}
            nearest={nearestSessionDay(day, around)}
            onPick={onPick}
            onNew={onNew}
          />
        </div>
      )}
    </>
  );
}
