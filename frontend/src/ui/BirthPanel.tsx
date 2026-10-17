import { useState } from "react";
import { monthsBefore } from "@/lib/dates";
import { DayMonthYearWheels } from "./PickerWheels";
import styles from "./Picker.module.css";

type Props = {
  value: string;
  today: string;
  min: string;
  max: string;
  onPick: (day: string) => void;
  onClose: () => void;
  onClear?: () => void;
};

// Day, month and year wheels for a date far back, such as a birthday, on a touch screen. With no date yet it
// starts 30 years back, so the year wheel does not have to be rolled from this year.
export function BirthPanel({
  value,
  today,
  min,
  max,
  onPick,
  onClose,
  onClear,
}: Props) {
  const [draft, setDraft] = useState(value || monthsBefore(today, 360));
  return (
    <div className={styles.panel}>
      <DayMonthYearWheels
        day={draft}
        years={[Number(min.slice(0, 4)), Number(max.slice(0, 4))]}
        onChange={setDraft}
        autoFocus
      />
      <div className={styles.foot}>
        <div>
          {onClear && (
            <button type="button" className={styles.tx} onClick={onClear}>
              Clear
            </button>
          )}
        </div>
        <div>
          <button type="button" className={styles.tx} onClick={onClose}>
            Cancel
          </button>
          <button
            type="button"
            className={`${styles.tx} ${styles.done}`}
            onClick={() => onPick(draft)}
          >
            Done
          </button>
        </div>
      </div>
    </div>
  );
}
