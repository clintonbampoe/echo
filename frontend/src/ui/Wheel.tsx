import { useId, useRef, type CSSProperties, type ReactNode } from "react";
import type { Option } from "./selectTypes";
import { useWheel } from "./useWheel";
import styles from "./Wheel.module.css";

type Props = {
  // Names the wheel for screen readers, e.g. "Hour".
  label: string;
  options: readonly Option[];
  value: string;
  onChange: (value: string) => void;
  // Gives focus here when a picker opens, so the keyboard works at once.
  autoFocus?: boolean;
};

// A column of values you roll through, like the wheels of an iPhone timer: the same spring and pace as the day
// strip. The row in the middle slot is the picked one. Drag, flick, spin the mouse wheel, press the arrow keys,
// type a number, or tap a row. It is also a plain list for screen readers: every row is an option.
export function Wheel({ label, options, value, onChange, autoFocus }: Props) {
  const track = useRef<HTMLDivElement>(null);
  const id = useId();
  const index = Math.max(
    0,
    options.findIndex((o) => o.value === value),
  );
  const { pick, handlers } = useWheel(
    track,
    options.map((o) => o.label),
    index,
    (i) => {
      const option = options[i];
      if (option) onChange(option.value);
    },
  );
  return (
    <div
      ref={track}
      role="listbox"
      tabIndex={0}
      aria-label={label}
      aria-activedescendant={`${id}-${index}`}
      data-autofocus={autoFocus || undefined}
      className={styles.wheel}
      {...handlers}
    >
      <div className={styles.column}>
        {options.map((o, i) => (
          <div
            key={o.value}
            id={`${id}-${i}`}
            role="option"
            aria-selected={i === index}
            data-row
            className={styles.row}
            style={{ "--i": i } as CSSProperties}
            onClick={() => pick(i)}
          >
            {o.label}
          </div>
        ))}
      </div>
    </div>
  );
}

// Wheels side by side behind one black slot, with a name above each. Rows are 40px, or 48px on a touch screen.
export function WheelSet({
  heads,
  children,
}: {
  heads: readonly string[];
  children: ReactNode;
}) {
  return (
    <div className={styles.set}>
      <div className={styles.heads} aria-hidden="true">
        {heads.map((h, i) => (
          <span key={`${h}${i}`}>{h}</span>
        ))}
      </div>
      <div className={styles.stage}>
        <span className={styles.slot} aria-hidden="true" />
        {children}
      </div>
    </div>
  );
}
