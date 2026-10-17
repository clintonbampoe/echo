import { useState } from "react";
import { monthNames, monthsAfter } from "@/lib/dates";
import { Calendar } from "./Calendar";
import { Icon } from "./Icon";
import { MonthYearWheels } from "./PickerWheels";
import styles from "./Picker.module.css";

type Props = {
  value: string;
  today: string;
  min: string;
  max: string;
  marks?: ReadonlySet<string>;
  range?: { from: string; to: string };
  onPick: (day: string) => void;
  // Offered when the field may be left empty.
  onClear?: () => void;
};

type Wheels = { year: number; month: number };

const pad = (n: number) => String(n).padStart(2, "0");
const yearOf = (day: string) => Number(day.slice(0, 4));

// The month the calendar opens on: the picked day's, else today's, kept between the earliest and latest day.
function startMonth({
  value,
  today,
  min,
  max,
}: Pick<Props, "value" | "today" | "min" | "max">): string {
  const day = value || today;
  return (day < min ? min : day > max ? max : day).slice(0, 7);
}

type HeadProps = {
  month: string;
  rolling: boolean;
  min: string;
  max: string;
  onMonth: (month: string) => void;
  onToggle: () => void;
};

// The month's name, which opens the wheels, with an arrow each side to step a month (hidden while the wheels show).
function Head({ month, rolling, min, max, onMonth, onToggle }: HeadProps) {
  const [year = 0, m = 1] = month.split("-").map(Number);
  return (
    <div className={styles.head}>
      {!rolling && (
        <button
          type="button"
          className={styles.step}
          aria-label="Previous month"
          disabled={month <= min.slice(0, 7)}
          onClick={() => onMonth(monthsAfter(month, -1))}
        >
          <Icon name="arrow-left" size={16} />
        </button>
      )}
      <button
        type="button"
        className={styles.my}
        aria-expanded={rolling}
        onClick={onToggle}
      >
        {monthNames[m - 1]} {year}
        <Icon name="chevron-down" size={14} />
      </button>
      {!rolling && (
        <button
          type="button"
          className={styles.step}
          aria-label="Next month"
          disabled={month >= max.slice(0, 7)}
          onClick={() => onMonth(monthsAfter(month, 1))}
        >
          <Icon name="arrow-right" size={16} />
        </button>
      )}
    </div>
  );
}

type RollProps = {
  wheels: Wheels;
  min: string;
  max: string;
  onChange: (wheels: Wheels) => void;
  onCancel: () => void;
  onDone: () => void;
};

// Month and year wheels, kept as a draft until Done.
function Roll({ wheels, min, max, onChange, onCancel, onDone }: RollProps) {
  return (
    <>
      <MonthYearWheels
        year={wheels.year}
        month={wheels.month}
        years={[yearOf(min), yearOf(max)]}
        onChange={(year, month) => onChange({ year, month })}
        autoFocus
      />
      <div className={styles.foot}>
        <div />
        <div>
          <button type="button" className={styles.tx} onClick={onCancel}>
            Cancel
          </button>
          <button
            type="button"
            className={`${styles.tx} ${styles.done}`}
            onClick={onDone}
          >
            Done
          </button>
        </div>
      </div>
    </>
  );
}

type DaysProps = Props & { month: string; onMonth: (month: string) => void };

// The month's days, with Today and Clear underneath.
function Days({ month, onMonth, onClear, ...props }: DaysProps) {
  const { value, today, min, max, onPick } = props;
  return (
    <>
      <Calendar
        month={month}
        value={value}
        today={today}
        min={min}
        max={max}
        marks={props.marks}
        range={props.range}
        onPick={onPick}
        onMonth={onMonth}
      />
      <div className={styles.foot}>
        <div>
          <button
            type="button"
            className={styles.tx}
            disabled={today < min || today > max}
            onClick={() => onPick(today)}
          >
            Today
          </button>
        </div>
        <div>
          {onClear && (
            <button type="button" className={styles.tx} onClick={onClear}>
              Clear
            </button>
          )}
        </div>
      </div>
    </>
  );
}

// The calendar popover or sheet: a month at a time, with arrows. Tap the month's name to roll to another month
// and year with wheels. Picking a day is the answer, so there is no Done.
export function DatePanel(props: Props) {
  const { min, max } = props;
  const [month, setMonth] = useState(startMonth(props));
  const [wheels, setWheels] = useState<Wheels | null>(null);

  function toggle() {
    const [year = 0, m = 1] = month.split("-").map(Number);
    setWheels(wheels ? null : { year, month: m });
  }

  function done() {
    if (!wheels) return;
    const next = `${wheels.year}-${pad(wheels.month)}`;
    setMonth(
      next < min.slice(0, 7)
        ? min.slice(0, 7)
        : next > max.slice(0, 7)
          ? max.slice(0, 7)
          : next,
    );
    setWheels(null);
  }

  return (
    <div className={styles.panel}>
      <Head
        month={month}
        rolling={Boolean(wheels)}
        min={min}
        max={max}
        onMonth={setMonth}
        onToggle={toggle}
      />
      {wheels ? (
        <Roll
          wheels={wheels}
          min={min}
          max={max}
          onChange={setWheels}
          onCancel={() => setWheels(null)}
          onDone={done}
        />
      ) : (
        <Days {...props} month={month} onMonth={setMonth} />
      )}
    </div>
  );
}
