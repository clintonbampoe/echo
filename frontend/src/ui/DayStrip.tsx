import { useRef, type CSSProperties } from "react";
import { Icon } from "./Icon";
import { useDayDial } from "./useDayDial";
import styles from "./DayStrip.module.css";

// One day on the strip. `marks` is how many things (sessions, events) fall on it; `ahead` greys the dots of a day still to come.
// `tag` is a short line above the weekday ("Today", or the month on its first day).
export type StripDay = {
  day: string;
  weekday: string;
  date: number;
  marks: number;
  today: boolean;
  ahead?: boolean;
  tag?: string;
};

type Props = {
  // Names the strip for screen readers, e.g. "Days".
  label: string;
  days: StripDay[];
  selected: string;
  onSelect: (day: string) => void;
  // The arrows move the picked day a week back or forward.
  onShift: (weeks: -1 | 1) => void;
  // What one marked day is called, e.g. ['session', 'sessions']. It only shows in the day's spoken name.
  noun: [one: string, many: string];
};

function DayCard({
  d,
  i,
  on,
  noun,
  onSelect,
}: {
  d: StripDay;
  i: number;
  on: boolean;
  noun: Props["noun"];
  onSelect: (day: string) => void;
}) {
  const spoken = `${d.weekday} ${d.date}${d.today ? ", today" : ""}${d.marks > 0 ? `, ${d.marks} ${d.marks === 1 ? noun[0] : noun[1]}` : ""}`;
  return (
    <li
      data-day={d.day}
      className={styles.item}
      style={{ "--i": i } as CSSProperties}
    >
      <button
        type="button"
        aria-label={spoken}
        aria-pressed={on}
        tabIndex={on ? 0 : -1}
        className={`${styles.day} ${d.today ? styles.today : ""}`}
        onClick={() => onSelect(d.day)}
      >
        <span className={styles.tag} aria-hidden="true">
          {d.tag}
        </span>
        <span className={styles.weekday}>{d.weekday}</span>
        <span className={styles.date}>{d.date}</span>
        <span
          className={`${styles.marks} ${d.ahead ? styles.ahead : ""}`}
          aria-hidden="true"
        >
          {d.marks > 0 && <span className={styles.dot} />}
          {d.marks > 1 && <span className={styles.dot} />}
        </span>
      </button>
    </li>
  );
}

// A dial of days you roll through, like the wheels of an iPhone timer. The day in the middle slot is the picked
// one: swipe, drag, spin the wheel or press the arrow keys and the row runs on and lands a day in the slot, which
// is picked; tap a day, or use the arrows, and it rolls into the slot. A dot marks a day that has something on it.
export function DayStrip({
  label,
  days,
  selected,
  onSelect,
  onShift,
  noun,
}: Props) {
  const track = useRef<HTMLOListElement>(null);
  const handlers = useDayDial(
    track,
    days.map((d) => d.day),
    selected,
    onSelect,
  );

  return (
    <div role="group" aria-label={label} className={styles.strip}>
      <button
        type="button"
        aria-label="Previous week"
        className={styles.arrow}
        onClick={() => onShift(-1)}
      >
        <Icon name="arrow-left" size={16} />
      </button>
      <div className={styles.stage}>
        <ol ref={track} className={styles.track} {...handlers}>
          {days.map((d, i) => (
            <DayCard
              key={d.day}
              d={d}
              i={i}
              on={d.day === selected}
              noun={noun}
              onSelect={onSelect}
            />
          ))}
        </ol>
      </div>
      <button
        type="button"
        aria-label="Next week"
        className={styles.arrow}
        onClick={() => onShift(1)}
      >
        <Icon name="arrow-right" size={16} />
      </button>
    </div>
  );
}
