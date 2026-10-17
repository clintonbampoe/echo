import { useState } from "react";
import { Button, Chip, DatePanel, Icon, PickerAnchor } from "@/ui";
import { longDay, sessionCount } from "../day";
import styles from "./DayBar.module.css";

type Props = {
  day: string;
  today: string;
  count: number | null;
  onPick: (day: string) => void;
};

// The picked day written out, how many sessions it has, and the two ways to jump: to today, or to any date.
export function DayBar({ day, today, count, onPick }: Props) {
  const [open, setOpen] = useState(false);

  function pick(next: string) {
    onPick(next);
    setOpen(false);
  }

  const panel = (
    <DatePanel
      value={day}
      today={today}
      min="2000-01-01"
      max={`${Number(today.slice(0, 4)) + 5}-12-31`}
      onPick={pick}
    />
  );
  return (
    <div className={styles.bar}>
      <h2 className={styles.heading}>
        {longDay(day)}
        {count !== null && <Chip tone="neutral">{sessionCount(count)}</Chip>}
      </h2>
      <div className={styles.buttons}>
        <Button
          variant="secondary"
          disabled={day === today}
          onClick={() => onPick(today)}
        >
          Today
        </Button>
        <PickerAnchor
          open={open}
          onClose={() => setOpen(false)}
          title="Pick a date"
          panel={panel}
          className={styles.pick}
        >
          <Button
            variant="secondary"
            data-trigger
            aria-haspopup="dialog"
            aria-expanded={open}
            onClick={() => setOpen(!open)}
          >
            <Icon name="calendar" size={16} />
            Pick a date
          </Button>
        </PickerAnchor>
      </div>
    </div>
  );
}
