import { useRef } from "react";
import { formatDate } from "@/lib/format";
import { Icon } from "./Icon";
import styles from "./DatePill.module.css";

type Props = {
  // What the day is, e.g. "From". It shows while no day is chosen and is the start of the spoken name.
  label: string;
  // A day as "2026-10-11", or '' for none yet.
  value: string;
  onChange: (day: string) => void;
};

// A pill that shows one day and opens the device's own date picker. Same look as the other filter pills.
export function DatePill({ label, value, onChange }: Props) {
  const input = useRef<HTMLInputElement>(null);
  return (
    <span className={styles.wrap}>
      <button
        type="button"
        aria-label={`${label}: ${value ? formatDate(value) : "any day"}`}
        className={`${styles.pill} ${value ? styles.on : ""}`}
        onClick={() => input.current?.showPicker?.()}
      >
        {value ? `${label} ${formatDate(value)}` : label}
        <Icon name="calendar" size={16} />
      </button>
      <input
        ref={input}
        type="date"
        tabIndex={-1}
        aria-hidden="true"
        className={styles.hidden}
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
    </span>
  );
}
