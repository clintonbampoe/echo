import { useState } from "react";
import { timeNow } from "@/lib/time";
import { TimeWheels } from "./PickerWheels";
import { useFinePointer } from "./useFinePointer";
import styles from "./Picker.module.css";

type Props = {
  value: string;
  onPick: (time: string) => void;
  onClose: () => void;
  // Offered when the field may be left empty.
  onClear?: () => void;
};

// The time wheels in a popover or sheet. The wheels change a draft; Done keeps it. On a touch screen there is a
// Cancel too, since the sheet has no other close button.
export function TimePanel({ value, onPick, onClose, onClear }: Props) {
  const fine = useFinePointer();
  const [draft, setDraft] = useState(value || "09:00");
  return (
    <div className={styles.panel}>
      <TimeWheels value={draft} onChange={setDraft} autoFocus />
      <div className={styles.foot}>
        <div>
          <button
            type="button"
            className={styles.tx}
            onClick={() => setDraft(timeNow())}
          >
            Now
          </button>
          {onClear && (
            <button type="button" className={styles.tx} onClick={onClear}>
              Clear
            </button>
          )}
        </div>
        <div>
          {!fine && (
            <button type="button" className={styles.tx} onClick={onClose}>
              Cancel
            </button>
          )}
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
