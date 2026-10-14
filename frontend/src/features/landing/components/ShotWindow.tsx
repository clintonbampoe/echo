import { SHOT_HEIGHT, SHOT_WIDTH, type Shot } from "../content";
import styles from "./ShotWindow.module.css";

type Props = {
  shot: Shot;
  // The first screen on the page loads straight away; the rest wait until they are seen.
  priority?: boolean;
};

// A screenshot inside a window frame. Two sizes are offered so sharp screens get the sharp one.
export function ShotWindow({ shot, priority }: Props) {
  return (
    <div className={styles.window}>
      <div className={styles.dots} aria-hidden="true">
        <i />
        <i />
        <i />
      </div>
      <img
        src={shot.x1}
        srcSet={`${shot.x1} ${SHOT_WIDTH}w, ${shot.x2} ${SHOT_WIDTH * 2}w`}
        sizes="(min-width: 1120px) 1036px, calc(100vw - 84px)"
        width={SHOT_WIDTH}
        height={SHOT_HEIGHT}
        alt={shot.alt}
        loading={priority ? "eager" : "lazy"}
        fetchPriority={priority ? "high" : "auto"}
        decoding="async"
        className={styles.image}
      />
    </div>
  );
}
