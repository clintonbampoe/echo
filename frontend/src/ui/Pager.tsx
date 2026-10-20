import {
  Children,
  useRef,
  useState,
  type CSSProperties,
  type ReactNode,
} from "react";
import styles from "./Pager.module.css";

type Props = {
  // Names the group for screen readers, e.g. "Money".
  label: string;
  // One name per child, in order, for the dots: "Show Income and expenses".
  titles: readonly string[];
  // The widths side by side on a wide screen, as a CSS grid value ("3fr 2fr"). Equal by default.
  columns?: string;
  children: ReactNode;
};

// Related cards. On a wide screen they sit side by side; on a phone they become pages to swipe through, with
// a dot for each underneath. Each child keeps its own card.
export function Pager({ label, titles, columns, children }: Props) {
  const slides = Children.toArray(children);
  const track = useRef<HTMLDivElement>(null);
  const [at, setAt] = useState(0);

  // The page whose left edge is nearest to where the track is scrolled to.
  const onScroll = () => {
    const el = track.current;
    if (!el) return;
    const lefts = Array.from(el.children).flatMap((c) =>
      c instanceof HTMLElement ? [c.offsetLeft] : [],
    );
    let nearest = 0;
    lefts.forEach((left, i) => {
      if (
        Math.abs(left - el.scrollLeft) <
        Math.abs((lefts[nearest] ?? 0) - el.scrollLeft)
      )
        nearest = i;
    });
    setAt(nearest);
  };

  const go = (i: number) => {
    const el = track.current;
    const child = el?.children[i];
    if (el && child instanceof HTMLElement && typeof el.scrollTo === "function")
      el.scrollTo({ left: child.offsetLeft });
    setAt(i);
  };

  const style = columns
    ? ({ "--columns": columns } as CSSProperties)
    : undefined;
  return (
    <div role="group" aria-label={label} className={styles.pager}>
      <div
        ref={track}
        className={styles.track}
        style={style}
        onScroll={onScroll}
      >
        {slides}
      </div>
      {slides.length > 1 && (
        <div className={styles.dots}>
          {slides.map((_, i) => (
            <button
              key={titles[i] ?? i}
              type="button"
              aria-label={`Show ${titles[i] ?? `view ${i + 1}`}`}
              aria-current={i === at}
              className={styles.dot}
              onClick={() => go(i)}
            />
          ))}
        </div>
      )}
    </div>
  );
}
