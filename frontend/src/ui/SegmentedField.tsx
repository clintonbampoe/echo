import { useId } from "react";
import styles from "./SegmentedField.module.css";

type Props<V extends string> = {
  label: string;
  options: readonly { value: V; label: string }[];
  value: V;
  onChange: (value: V) => void;
  // Chips of their own width that wrap onto the next line, for a longer list (five payment methods).
  wrap?: boolean;
};

// A few choices side by side, one of them picked: the buttons of a short list that should stay in view (a role, say).
// Real radio buttons underneath, so the arrow keys and screen readers work as they should.
export function SegmentedField<V extends string>({
  label,
  options,
  value,
  onChange,
  wrap,
}: Props<V>) {
  const name = useId();
  return (
    <fieldset className={styles.field}>
      <legend className={styles.label}>{label}</legend>
      <div className={wrap ? `${styles.group} ${styles.wrap}` : styles.group}>
        {options.map((o) => (
          <label key={o.value} className={styles.option}>
            <input
              type="radio"
              name={name}
              value={o.value}
              checked={o.value === value}
              onChange={() => onChange(o.value)}
              className={styles.input}
            />
            <span className={styles.face}>{o.label}</span>
          </label>
        ))}
      </div>
    </fieldset>
  );
}
