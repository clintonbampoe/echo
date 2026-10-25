import styles from "./Switch.module.css";

type Props = {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
};

// An on/off switch. A real button with role "switch", so keyboards and screen readers treat it as one.
export function Switch({ label, checked, onChange }: Props) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      aria-label={label}
      className={styles.track}
      onClick={() => onChange(!checked)}
    >
      <span className={styles.thumb} />
    </button>
  );
}
