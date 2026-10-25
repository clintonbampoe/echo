import { useIsFetching, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef, useState, type CSSProperties } from "react";
import { Icon } from "@/ui";
import styles from "./PullToRefresh.module.css";

// How far the page must be pulled, in px, before letting go refreshes it; and how far it can stretch.
const TRIGGER = 64;
const LIMIT = 96;
// A refresh shows for at least this long, so a quick one is still seen to happen.
const AT_LEAST = 600;

// Where a pull must not start: forms and anything floating over the page (feel spec: never on forms).
const SKIP = "form, input, textarea, select, [role=dialog], [role=menu]";

// Pulling the page down from the top on a touch screen refreshes what it shows: every query on the page is
// fetched again. The page follows the finger with a rubber band; a spinner shows until the data is back.
export function PullToRefresh() {
  const client = useQueryClient();
  const fetching = useIsFetching();
  const [pull, setPull] = useState(0);
  const [asked, setAsked] = useState(false);
  const [shownLongEnough, setShownLongEnough] = useState(false);
  const from = useRef<{ x: number; y: number } | null>(null);
  const pulled = useRef(0);
  const timer = useRef<number | undefined>(undefined);

  // The browser's own pull-to-refresh would reload the whole app: it is held back while this one is mounted.
  useEffect(() => {
    const root = document.documentElement;
    const before = root.style.overscrollBehaviorY;
    root.style.overscrollBehaviorY = "contain";
    return () => {
      root.style.overscrollBehaviorY = before;
    };
  }, []);

  useEffect(() => {
    const move = (to: number) => {
      pulled.current = to;
      setPull(to);
    };
    const onStart = (e: TouchEvent) => {
      const t = e.touches[0];
      const skip = e.target instanceof Element && e.target.closest(SKIP);
      from.current =
        window.scrollY <= 0 && !skip && t
          ? { x: t.clientX, y: t.clientY }
          : null;
    };
    const onMove = (e: TouchEvent) => {
      const s = from.current;
      const t = e.touches[0];
      if (!s || !t) return;
      const dy = t.clientY - s.y;
      // Sideways or upwards: the finger is doing something else.
      if (dy <= 0 || Math.abs(t.clientX - s.x) > dy) return move(0);
      move(Math.min(LIMIT, dy * 0.5));
    };
    const onEnd = () => {
      if (!from.current) return;
      from.current = null;
      const ready = pulled.current >= TRIGGER;
      move(0);
      if (!ready) return;
      setAsked(true);
      setShownLongEnough(false);
      window.clearTimeout(timer.current);
      timer.current = window.setTimeout(
        () => setShownLongEnough(true),
        AT_LEAST,
      );
      void client.invalidateQueries();
    };
    document.addEventListener("touchstart", onStart, { passive: true });
    document.addEventListener("touchmove", onMove, { passive: true });
    document.addEventListener("touchend", onEnd);
    document.addEventListener("touchcancel", onEnd);
    return () => {
      document.removeEventListener("touchstart", onStart);
      document.removeEventListener("touchmove", onMove);
      document.removeEventListener("touchend", onEnd);
      document.removeEventListener("touchcancel", onEnd);
      window.clearTimeout(timer.current);
    };
  }, [client]);

  const busy = asked && (!shownLongEnough || fetching > 0);
  const height = busy ? TRIGGER * 0.75 : pull;
  return (
    <div
      className={styles.pull}
      data-shown={height > 0}
      data-busy={busy}
      data-dragging={(pull > 0 && !busy) || undefined}
      data-ready={pull >= TRIGGER}
      style={{ "--pull": `${height}px` } as CSSProperties}
      role="status"
      aria-label={busy ? "Refreshing" : undefined}
    >
      <span className={styles.dot}>
        <Icon
          name="spinner"
          size={20}
          className={busy ? "anim-spin" : undefined}
        />
      </span>
    </div>
  );
}
