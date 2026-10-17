import { useState } from "react";
import { monthNames } from "@/lib/dates";
import { Icon } from "./Icon";
import { PickerAnchor } from "./PickerAnchor";
import { MonthYearWheels } from "./PickerWheels";
import styles from "./MonthYearPill.module.css";
import pickerStyles from "./Picker.module.css";

type Props = {
  // The word on the pill while nothing is chosen, e.g. "Tithe for".
  label: string;
  // '' for any, '2026' for a whole year, '2026-10' for one month.
  value: string;
  onChange: (value: string) => void;
  // Today as a plain day. A new picker opens on its month.
  today: string;
};

function describe(value: string): string {
  const [year = "", month] = value.split("-");
  return month ? `${monthNames[Number(month) - 1] ?? ""} ${year}` : year;
}

// A pill that narrows a list to one month, or one whole year. The two wheels (month, year) change a draft; Done
// keeps a month, Whole year keeps the year alone, Clear takes the filter off.
export function MonthYearPill({ label, value, onChange, today }: Props) {
  const [open, setOpen] = useState(false);

  function choose(next: string) {
    onChange(next);
    setOpen(false);
  }

  const panel = <Panel value={value} today={today} onChoose={choose} />;
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
        className={`${styles.pill} ${value ? styles.on : ""}`}
        onClick={() => setOpen(!open)}
      >
        {value ? `${label}: ${describe(value)}` : label}
        <Icon name="chevrons-up-down" size={14} className={styles.chevron} />
      </button>
    </PickerAnchor>
  );
}

function Panel({
  value,
  today,
  onChoose,
}: {
  value: string;
  today: string;
  onChoose: (value: string) => void;
}) {
  const [valueYear, valueMonth] = value.split("-");
  const thisYear = Number(today.slice(0, 4));
  const [draft, setDraft] = useState({
    year: Number(valueYear || thisYear),
    month: Number(valueMonth || today.slice(5, 7)),
  });
  return (
    <div className={pickerStyles.panel}>
      <MonthYearWheels
        year={draft.year}
        month={draft.month}
        years={[2000, thisYear + 1]}
        onChange={(year, month) => setDraft({ year, month })}
        autoFocus
      />
      <div className={pickerStyles.foot}>
        <div>
          <button
            type="button"
            className={pickerStyles.tx}
            onClick={() => onChoose("")}
          >
            Clear
          </button>
        </div>
        <div>
          <button
            type="button"
            className={pickerStyles.tx}
            onClick={() => onChoose(String(draft.year))}
          >
            Whole year
          </button>
          <button
            type="button"
            className={`${pickerStyles.tx} ${pickerStyles.done}`}
            onClick={() =>
              onChoose(`${draft.year}-${String(draft.month).padStart(2, "0")}`)
            }
          >
            Done
          </button>
        </div>
      </div>
    </div>
  );
}
