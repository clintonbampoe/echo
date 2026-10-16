import { useId, useState, type FocusEvent } from "react";
import { monthNames } from "@/lib/dates";
import { BottomSheet } from "./BottomSheet";
import { Icon } from "./Icon";
import { useFinePointer } from "./useFinePointer";
import styles from "./MonthYearPill.module.css";

type Props = {
  // The word on the pill while nothing is chosen, e.g. "Tithe for".
  label: string;
  // '' for any, '2026' for a whole year, '2026-10' for one month.
  value: string;
  onChange: (value: string) => void;
  // Today as a plain day. A new picker opens on its year.
  today: string;
};

const short = monthNames.map((m) => m.slice(0, 3));

function describe(value: string): string {
  const [year = "", month] = value.split("-");
  return month ? `${monthNames[Number(month) - 1] ?? ""} ${year}` : year;
}

// A pill that narrows a list to one month, or one whole year. With a mouse the picker hangs under the pill;
// on a touch screen it rises from the bottom.
export function MonthYearPill({ label, value, onChange, today }: Props) {
  const id = useId();
  const [open, setOpen] = useState(false);
  const fine = useFinePointer();

  function choose(next: string) {
    onChange(next);
    setOpen(false);
  }

  function onFocusOut(e: FocusEvent<HTMLDivElement>) {
    if (fine && !e.currentTarget.contains(e.relatedTarget)) setOpen(false);
  }

  const picker = (
    <Picker label={label} value={value} today={today} onChoose={choose} />
  );
  return (
    <div className={styles.wrap} onBlur={onFocusOut}>
      <button
        type="button"
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-controls={fine ? id : undefined}
        className={`${styles.pill} ${value ? styles.on : ""}`}
        onClick={() => setOpen(!open)}
      >
        {value ? `${label}: ${describe(value)}` : label}
        <Icon name="chevrons-up-down" size={14} className={styles.chevron} />
      </button>
      {fine && open && (
        <div
          id={id}
          role="dialog"
          aria-label={label}
          className={styles.popover}
          onKeyDown={(e) => e.key === "Escape" && setOpen(false)}
        >
          {picker}
        </div>
      )}
      {!fine && (
        <BottomSheet open={open} onClose={() => setOpen(false)} title={label}>
          {picker}
        </BottomSheet>
      )}
    </div>
  );
}

type PickerProps = {
  label: string;
  value: string;
  today: string;
  onChoose: (value: string) => void;
};

function Picker({ label, value, today, onChoose }: PickerProps) {
  const [chosenYear = "", chosenMonth] = value.split("-");
  const [year, setYear] = useState(Number(chosenYear || today.slice(0, 4)));
  const key = (month: number) => `${year}-${String(month).padStart(2, "0")}`;
  return (
    <div className={styles.picker}>
      <div className={styles.years}>
        <button
          type="button"
          className={styles.step}
          aria-label={`${label}: previous year`}
          onClick={() => setYear(year - 1)}
        >
          <Icon name="arrow-left" size={16} />
        </button>
        <span aria-live="polite" className={styles.year}>
          {year}
        </span>
        <button
          type="button"
          className={styles.step}
          aria-label={`${label}: next year`}
          onClick={() => setYear(year + 1)}
        >
          <Icon name="arrow-right" size={16} />
        </button>
      </div>
      <div className={styles.months}>
        {short.map((name, i) => (
          <button
            key={name}
            type="button"
            aria-label={`${monthNames[i]} ${year}`}
            aria-pressed={
              chosenYear === String(year) && chosenMonth === key(i + 1).slice(5)
            }
            className={styles.month}
            onClick={() => onChoose(key(i + 1))}
          >
            {name}
          </button>
        ))}
      </div>
      <div className={styles.foot}>
        <button
          type="button"
          className={styles.action}
          onClick={() => onChoose(String(year))}
        >
          Whole year
        </button>
        <button
          type="button"
          className={styles.action}
          onClick={() => onChoose("")}
        >
          Clear
        </button>
      </div>
    </div>
  );
}
