import { useState } from "react";
import { daysAfter, monthGrid, monthsAfter } from "@/lib/dates";
import { formatDayTime } from "@/lib/format";
import { Icon } from "@/ui";
import { useEventsBetween } from "../hooks";
import {
  defaultPick,
  detailOf,
  eventDays,
  kindOf,
  monthTitle,
} from "../events";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import styles from "./EventCalendarCard.module.css";

const weekdays = ["S", "M", "T", "W", "T", "F", "S"];

type DayProps = {
  day: string;
  month: string;
  today: string;
  // The events on this day, when it has any.
  names: string[] | undefined;
  picked: boolean;
  onPick: (day: string) => void;
};

// One cell. A day outside the month is blank, so the grid keeps six full rows. A day with an event is a
// button, because it can be picked.
function Day({ day, month, today, names, picked, onPick }: DayProps) {
  if (!day.startsWith(month))
    return <span className={styles.blank} aria-hidden="true" />;
  const number = Number(day.slice(8));
  const className = [
    styles.day,
    day === today && styles.today,
    names && styles[kindOf(day, today)],
    picked && styles.picked,
  ]
    .filter(Boolean)
    .join(" ");
  if (!names) return <span className={className}>{number}</span>;
  return (
    <button
      type="button"
      className={className}
      aria-label={`${formatDayTime(day)}, ${names.join(", ")}`}
      aria-pressed={picked}
      aria-current={day === today ? "date" : undefined}
      onClick={() => onPick(day)}
    >
      {number}
    </button>
  );
}

// A month of days with the ones that have an event marked: filled dark when still to come, filled light when
// held, and a ring on today. The line under the grid says what is on the day picked.
export function EventCalendarCard({ today }: { today: string }) {
  const [month, setMonth] = useState(today.slice(0, 7));
  const [picked, setPicked] = useState<string>();
  const grid = monthGrid(month);
  const data = useEventsBetween(
    grid[0] ?? month,
    grid[41] ?? daysAfter(month, 41),
  );
  const days = eventDays(data.events ?? [], month);
  const chosen = picked && days.has(picked) ? picked : defaultPick(days, today);

  const step = (by: number) => setMonth(monthsAfter(month, by));
  const nav = (
    label: string,
    by: number,
    icon: "arrow-left" | "arrow-right",
  ) => (
    <button
      type="button"
      className={styles.step}
      aria-label={label}
      onClick={() => step(by)}
    >
      <Icon name={icon} size={16} />
    </button>
  );

  return (
    <DashCard
      title="Event calendar"
      hint="Days with an event are marked"
      to="/events"
      control={
        <div className={styles.nav}>
          {nav("Previous month", -1, "arrow-left")}
          {nav("Next month", 1, "arrow-right")}
        </div>
      }
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content} aria-busy={!data.events}>
          <p className={styles.month}>{monthTitle(month)}</p>
          <div className={styles.weekdays} aria-hidden="true">
            {weekdays.map((d, i) => (
              <span key={i}>{d}</span>
            ))}
          </div>
          <div
            role="group"
            aria-label={monthTitle(month)}
            className={styles.days}
          >
            {grid.map((day) => (
              <Day
                key={day}
                day={day}
                month={month}
                today={today}
                names={days.get(day)}
                picked={day === chosen}
                onPick={setPicked}
              />
            ))}
          </div>
          <div className={styles.foot}>
            <span className={styles.key}>
              <span>
                <i className={styles.upcoming} />
                Upcoming
              </span>
              <span>
                <i className={styles.held} />
                Held
              </span>
            </span>
            <span className={styles.detail}>
              {detailOf(chosen, days, data.events !== undefined)}
            </span>
          </div>
        </div>
      )}
    </DashCard>
  );
}
