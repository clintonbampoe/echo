import styles from "./Logo.module.css";

type Props = {
  size?: number;
  // A thin ring that draws around the mark as the page is scrolled (browsers that can tie an animation to scrolling).
  progress?: boolean;
};

// Echo's mark: a point with two rings spreading out from it.
export function Logo({ size = 40, progress }: Props) {
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
      {progress && (
        <circle
          cx="12"
          cy="12"
          r="10.5"
          pathLength="100"
          className={styles.progress}
        />
      )}
    </svg>
  );
}
