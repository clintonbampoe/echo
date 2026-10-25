import { useState } from "react";
import { daysAfter, monthGrid, monthsAfter } from "@/lib/dates";
import { formatDayTime } from "@/lib/format";
import { DashCard, DateTile, Icon } from "@/ui";
import { useEventsBetween } from "../hooks";
import { defaultPick, eventDays, kindOf, monthTitle } from "../events";
import { CardFailed } from "./CardFailed";
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

// What is on the day picked: its date in a tile, the event's name and the day in words.
function Chosen({
  day,
  names,
  ready,
}: {
  day: string | undefined;
  names: string[] | undefined;
  ready: boolean;
}) {
  const first = names?.[0];
  if (!day || !names || !first)
    return (
      <p className={styles.none}>{ready ? "No events this month" : "\u00a0"}</p>
    );
  return (
    <div className={styles.foot}>
      <DateTile date={day} />
      <span className={styles.detail}>
        <span className={styles.name}>
          {first}
          {names.length > 1 ? ` and ${names.length - 1} more` : ""}
        </span>
        <span className={styles.when}>{formatDayTime(day)}</span>
      </span>
    </div>
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
    icon: "chevron-left" | "chevron-right",
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

  const names = chosen ? days.get(chosen) : undefined;

  return (
    <DashCard
      title="Event calendar"
      control={
        <div className={styles.nav}>
          <span className={styles.month}>{monthTitle(month)}</span>
          {nav("Previous month", -1, "chevron-left")}
          {nav("Next month", 1, "chevron-right")}
        </div>
      }
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content} aria-busy={!data.events}>
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
          <Chosen day={chosen} names={names} ready={!!data.events} />
        </div>
      )}
    </DashCard>
  );
}
