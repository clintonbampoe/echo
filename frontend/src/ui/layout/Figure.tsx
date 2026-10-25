import styles from "./Figure.module.css";

type Props = {
  // The line over the number: the month it is for, "October 2026".
  label: string;
  value: string;
  // The small word after the number: "present", "GHS".
  unit?: string;
  // Smaller numbers under it, e.g. members and visitors of the people present.
  sides?: readonly { label: string; value: string }[];
};

// The number a chart card speaks about, as the boards draw it: what it is for, the figure big, and what it is
// made of in two small ones.
export function Figure({ label, value, unit, sides }: Props) {
  return (
    <div className={styles.figure}>
      <p className={styles.label}>{label}</p>
      <p className={styles.big}>
        {value}
        {unit && <span className={styles.unit}>{unit}</span>}
      </p>
      {sides && (
        <div role="group" aria-label="Made up of" className={styles.sides}>
          {sides.map((s) => (
            <div key={s.label}>
              <p className={styles.label}>{s.label}</p>
              <p className={styles.side}>{s.value}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
