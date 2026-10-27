import styles from "./ProgressBar.module.css";

type Props = {
  // How far along, as a fraction: 0.6 is 60%. Past 1 the bar is full.
  value: number;
  // What it measures, for screen readers: "Raised against target".
  label: string;
};

// A thin bar showing how much of a goal is reached, e.g. money raised against a target.
export function ProgressBar({ value, label }: Props) {
  const share = Math.max(0, Math.min(1, value));
  return (
    <div
      role="progressbar"
      aria-label={label}
      aria-valuemin={0}
      aria-valuemax={100}
      aria-valuenow={Math.round(share * 100)}
      className={styles.track}
    >
      <span
        className={`${styles.fill} grow-x`}
        style={{ width: `${share * 100}%` }}
      />
    </div>
  );
}
