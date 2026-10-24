import styles from "./Logo.module.css";

type Props = {
  size?: number;
};

// Echo's mark: a point with two rings spreading out from it.
export function Logo({ size = 40 }: Props) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      role="img"
      aria-label="Echo"
      className={styles.logo}
    >
      <circle cx="12" cy="12" r="10.5" className={styles.outer} />
      <circle cx="12" cy="12" r="6.5" className={styles.inner} />
      <circle cx="12" cy="12" r="2.5" className={styles.core} />
    </svg>
  );
}
