import { useState, type CSSProperties } from "react";
import { useMedia } from "@/ui";
import { ROADMAP } from "../../content/home";
import styles from "./Roadmap.module.css";

// The roadmap as a road that scrolls: the step in front is full size with its detail showing, the others
// sit back, smaller and paler. Scrolling settles on one step at a time.
export function Roadmap() {
  const phone = useMedia("(max-width: 599px)");
  // On a touch screen the road is simply laid out in full and the page scrolls past it: a box that scrolls on its
  // own would take the swipe and stop the page under the finger.
  const touch = useMedia("(pointer: coarse)");
  const step = phone ? 200 : 220;
  const [active, setActive] = useState(0);
  return (
    <div
      className={styles.road}
      data-touch={touch || undefined}
      role="list"
      aria-label="Roadmap steps"
      tabIndex={0}
      onScroll={(e) => {
        const next = Math.round(e.currentTarget.scrollTop / step);
        setActive(Math.max(0, Math.min(ROADMAP.length - 1, next)));
      }}
    >
      <div className={styles.inner}>
        <span className={styles.rail} aria-hidden="true" />
        {ROADMAP.map((item, i) => {
          const gap = touch ? 0 : Math.abs(i - active);
          return (
            <div
              key={item.title}
              role="listitem"
              className={styles.step}
              data-state={item.state}
              data-active={gap === 0 || undefined}
              style={
                {
                  "--i": i,
                  opacity: gap === 0 ? 1 : gap === 1 ? 0.4 : 0.18,
                } as CSSProperties
              }
            >
              <span className={styles.halo} aria-hidden="true" />
              <span className={styles.dot} aria-hidden="true">
                <span />
              </span>
              <span className={styles.words}>
                <span className={styles.title}>{item.title}</span>
                <span className={styles.line}>{item.line}</span>
                <span className={styles.text}>{item.text}</span>
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}
