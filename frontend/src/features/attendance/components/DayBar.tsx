import { useRef } from "react";
import { Button, Chip, Icon } from "@/ui";
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
  const date = useRef<HTMLInputElement>(null);
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
        <span className={styles.pick}>
          <Button
            variant="secondary"
            onClick={() => date.current?.showPicker?.()}
          >
            <Icon name="calendar" size={16} />
            Pick a date
          </Button>
          <input
            ref={date}
            type="date"
            tabIndex={-1}
            aria-hidden="true"
            className={styles.hidden}
            value={day}
            onChange={(e) => e.target.value && onPick(e.target.value)}
          />
        </span>
      </div>
    </div>
  );
}
