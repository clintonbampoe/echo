import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import {
  daysAfter,
  monthGrid,
  monthsBefore,
  spokenDay,
  weekStart,
} from "@/lib/dates";
import styles from "./Calendar.module.css";

type Props = {
  // The month on show, as "2026-10".
  month: string;
  // The picked day, or '' for none.
  value: string;
  today: string;
  // Days outside these are shown but cannot be picked.
  min: string;
  max: string;
  // Days with something on them get a dot.
  marks?: ReadonlySet<string>;
  // A stretch of days drawn as a band, for a From and To pair.
  range?: { from: string; to: string };
  onPick: (day: string) => void;
  onMonth: (month: string) => void;
};

const weekdays = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
const clamp = (day: string, min: string, max: string) =>
  day < min ? min : day > max ? max : day;

// Where the arrow keys start: the picked day, else today, else the first of the month, when they are on show.
function start(month: string, value: string, today: string): string {
  return (
    [value, today, `${month}-01`].find((day) => day.startsWith(month)) ??
    `${month}-01`
  );
}

const KEYS: Record<string, (day: string) => string> = {
  ArrowLeft: (day) => daysAfter(day, -1),
  ArrowRight: (day) => daysAfter(day, 1),
  ArrowUp: (day) => daysAfter(day, -7),
  ArrowDown: (day) => daysAfter(day, 7),
  Home: weekStart,
  End: (day) => daysAfter(weekStart(day), 6),
  PageUp: (day) => monthsBefore(day, 1),
  PageDown: (day) => monthsBefore(day, -1),
};

type Band = { from: string; to: string };

// The band classes for a day: in the band, and whether it is the first or last of it.
function bandClass(day: string, band: Band): string {
  if (!band.from || day < band.from || day > band.to) return "";
  return [
    styles.band,
    day === band.from ? styles.first : "",
    day === band.to ? styles.last : "",
  ].join(" ");
}

function dayClass(
  day: string,
  month: string,
  today: string,
  value: string,
  band: Band,
): string {
  return [
    styles.day,
    day.startsWith(month) ? "" : styles.out,
    day === today ? styles.today : "",
    bandClass(day, band),
    day === value ? styles.picked : "",
  ]
    .filter(Boolean)
    .join(" ");
}

type DayProps = {
  day: string;
  className: string;
  spoken: string;
  picked: boolean;
  today: boolean;
  at: boolean;
  off: boolean;
  marked: boolean;
  onPick: (day: string) => void;
  onFocus: (day: string) => void;
};

function DayButton({
  day,
  className,
  spoken,
  picked,
  today,
  at,
  off,
  marked,
  onPick,
  onFocus,
}: DayProps) {
  return (
    <button
      type="button"
      data-day={day}
      data-autofocus={at || undefined}
      aria-label={spoken}
      aria-pressed={picked}
      aria-current={today ? "date" : undefined}
      tabIndex={at ? 0 : -1}
      disabled={off}
      className={className}
      onClick={() => onPick(day)}
      onFocus={() => onFocus(day)}
    >
      {Number(day.slice(8))}
      {marked && <i className={styles.dot} aria-hidden="true" />}
    </button>
  );
}

// A month as a grid of days, Sunday first. Always 6 rows, so it never changes height. It is read at a glance and
// tapped, so it does not roll like a wheel. The keyboard moves a day at a time; Page Up and Page Down move a month.
export function Calendar({
  month,
  value,
  today,
  min,
  max,
  marks,
  range,
  onPick,
  onMonth,
}: Props) {
  const grid = useRef<HTMLDivElement>(null);
  const [cursor, setCursor] = useState(start(month, value, today));
  const moved = useRef(false);
  // The day the keyboard is on, kept on the month on show.
  const at = cursor.startsWith(month) ? cursor : start(month, value, today);
  const [from = "", to = ""] =
    range?.from && range.to ? [range.from, range.to].sort() : [];

  // After a key moves the day (maybe into another month), focus follows it once it is on the page.
  useEffect(() => {
    if (!moved.current) return;
    moved.current = false;
    grid.current?.querySelector<HTMLElement>(`[data-day="${at}"]`)?.focus();
  });

  function onKeyDown(e: KeyboardEvent) {
    const go = KEYS[e.key];
    if (!go) return;
    e.preventDefault();
    const next = clamp(go(at), min, max);
    moved.current = true;
    setCursor(next);
    if (!next.startsWith(month)) onMonth(next.slice(0, 7));
  }

  return (
    <div ref={grid} role="group" aria-label="Calendar" onKeyDown={onKeyDown}>
      <div className={styles.weekdays} aria-hidden="true">
        {weekdays.map((d) => (
          <span key={d}>{d}</span>
        ))}
      </div>
      <div className={styles.days}>
        {monthGrid(month).map((day) => (
          <DayButton
            key={day}
            day={day}
            className={dayClass(day, month, today, value, { from, to })}
            spoken={spokenDay(day)}
            picked={day === value}
            today={day === today}
            at={day === at}
            off={day < min || day > max}
            marked={Boolean(marks?.has(day))}
            onPick={onPick}
            onFocus={setCursor}
          />
        ))}
      </div>
    </div>
  );
}
