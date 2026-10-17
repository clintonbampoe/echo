import { useState } from "react";
import { todayText } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { DatePanel } from "./DatePanel";
import { Icon } from "./Icon";
import { PickerAnchor } from "./PickerAnchor";
import styles from "./DatePill.module.css";

type Props = {
  // What the day is, e.g. "From". It shows while no day is chosen and is the start of the spoken name.
  label: string;
  // A day as "2026-10-11", or '' for none yet.
  value: string;
  onChange: (day: string) => void;
  // The other end of a From and To pair. When both are set the calendar draws the stretch between them.
  range?: { from: string; to: string };
};

// A pill that shows one day and opens our date picker. Same look as the other filter pills. A day left empty
// means no limit on that side, so the picker offers Clear.
export function DatePill({ label, value, onChange, range }: Props) {
  const [open, setOpen] = useState(false);
  const today = todayText();

  function pick(day: string) {
    onChange(day);
    setOpen(false);
  }

  const panel = (
    <DatePanel
      value={value}
      today={today}
      min="1900-01-01"
      max={`${Number(today.slice(0, 4)) + 5}-12-31`}
      range={range}
      onPick={pick}
      onClear={value ? () => pick("") : undefined}
    />
  );
  return (
    <PickerAnchor
      open={open}
      onClose={() => setOpen(false)}
      title={label}
      panel={panel}
      className={styles.wrap}
    >
      <button
        type="button"
        data-trigger
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-label={`${label}: ${value ? formatDate(value) : "any day"}`}
        className={`${styles.pill} ${value ? styles.on : ""}`}
        onClick={() => setOpen(!open)}
      >
        {value ? `${label} ${formatDate(value)}` : label}
        <Icon name="calendar" size={16} />
      </button>
    </PickerAnchor>
  );
}
