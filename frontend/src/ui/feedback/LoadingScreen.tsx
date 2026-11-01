import { Logo } from "../layout/Logo";
import styles from "./LoadingScreen.module.css";

type Props = {
  // Fills the window (the first page loading) or the space it is in (a page inside the app).
  fill?: "window" | "area";
};

// What shows while a page is on its way. The mark breathes, so it is plain the app is working and not stuck. It
// only appears after a short wait, so a page that arrives at once never flashes it.
export function LoadingScreen({ fill = "window" }: Props) {
  return (
    <div
      role="status"
      aria-label="Loading"
      className={styles.wrap}
      data-fill={fill}
    >
      <span className={styles.mark}>
        <Logo size={44} />
      </span>
    </div>
  );
}
