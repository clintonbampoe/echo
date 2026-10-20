import { useState } from "react";
import { Icon, PickerAnchor } from "@/ui";
import {
  periodOf,
  periods,
  rangeHint,
  spanOf,
  type PeriodKey,
} from "../periods";
import { moveFocus } from "./optionKeys";
import styles from "./PeriodPill.module.css";

type Props = {
  value: PeriodKey;
  today: string;
  onChange: (key: PeriodKey) => void;
};

// The dashboard's period control: a pill that opens the six periods, each with its dates. A popover on a
// mouse, a bottom sheet on a touch screen (PickerAnchor decides).
export function PeriodPill({ value, today, onChange }: Props) {
  const [open, setOpen] = useState(false);
  const current = periodOf(value);

  const pick = (key: PeriodKey) => {
    onChange(key);
    setOpen(false);
  };

  const panel = (
    <div
      role="listbox"
      aria-label="Period"
      className={styles.list}
      onKeyDown={moveFocus}
    >
      {periods.map((p) => {
        const selected = p.key === value;
        return (
          <button
            key={p.key}
            type="button"
            role="option"
            aria-selected={selected}
            tabIndex={selected ? 0 : -1}
            data-autofocus={selected ? "" : undefined}
            className={`${styles.option} ${selected ? styles.selected : ""}`}
            onClick={() => pick(p.key)}
          >
            <span className={styles.check}>
              {selected && <Icon name="check" size={16} />}
            </span>
            <span>{p.label}</span>
            <span className={styles.when}>
              {rangeHint(spanOf(p.key, today))}
            </span>
          </button>
        );
      })}
    </div>
  );

  return (
    <PickerAnchor
      open={open}
      onClose={() => setOpen(false)}
      title="Period"
      panel={panel}
      className={styles.anchor}
    >
      <button
        type="button"
        data-trigger
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={`Period: ${current.label}`}
        className={styles.pill}
        onClick={() => setOpen(!open)}
      >
        <Icon name="calendar" size={16} className={styles.icon} />
        {current.label}
        <span className={styles.range}>{rangeHint(spanOf(value, today))}</span>
        <Icon name="chevron-down" size={16} className={styles.chevron} />
      </button>
    </PickerAnchor>
  );
}
