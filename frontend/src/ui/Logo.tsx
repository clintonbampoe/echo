import styles from "./Logo.module.css";

type Props = {
  size?: number;
};

export function Logo({ size = 40 }: Props) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 40 40"
      role="img"
      aria-label="Echo"
      className={styles.logo}
    >
      <rect
        x="0.5"
        y="0.5"
        width="39"
        height="39"
        rx="11"
        className={styles.frame}
      />
      <text x="9" y="27" fontSize="20" fontWeight="600" className={styles.mark}>
        e
      </text>
      <path d="M23 14.5c2.2 3.2 2.2 8.8 0 12" className={styles.wave} />
      <path
        d="M27.5 11.5c3.6 4.6 3.6 12.4 0 17"
        className={`${styles.wave} ${styles.faint}`}
      />
    </svg>
  );
}
