import {
  useEffect,
  useLayoutEffect,
  useRef,
  type KeyboardEvent,
  type MouseEvent,
  type PointerEvent,
  type RefObject,
} from "react";
import {
  choose,
  dispose,
  down,
  key,
  move,
  newWheel,
  place,
  up,
  wheel,
  type Wheel,
} from "./wheelDial";

// Wires a wheel to a component: the picked row sits in the middle slot, and the column moves under a finger, the
// mouse, the wheel or the keys with one continuous motion. Returns the handlers for the column and `pick`, which
// a tap on a row uses.
export function useWheel(
  track: RefObject<HTMLDivElement | null>,
  labels: readonly string[],
  index: number,
  onPick: (index: number) => void,
) {
  const latest = useRef({ count: labels.length, value: index, onPick });
  const dial = useRef<Wheel | null>(null);
  const mounted = useRef(false);
  // Before anything below reads it.
  useLayoutEffect(() => {
    latest.current = { count: labels.length, value: index, onPick };
  });
  useLayoutEffect(() => {
    const made = newWheel(track, latest);
    dial.current = made;
    return () => dispose(made);
  }, [track]);
  useLayoutEffect(() => {
    if (!dial.current) return;
    place(dial.current, index, mounted.current);
    mounted.current = true;
  }, [index]);
  // A wheel needs a listener that may cancel the page scroll.
  useEffect(() => {
    const el = track.current;
    if (!el) return;
    const onWheel = (e: WheelEvent) => {
      e.preventDefault();
      if (dial.current) wheel(dial.current, e);
    };
    el.addEventListener("wheel", onWheel, { passive: false });
    return () => el.removeEventListener("wheel", onWheel);
  }, [track]);
  return {
    pick: (row: number) => dial.current && choose(dial.current, row),
    handlers: {
      onPointerDown: (e: PointerEvent<HTMLDivElement>) =>
        dial.current && down(dial.current, e),
      onPointerMove: (e: PointerEvent<HTMLDivElement>) => {
        if (
          dial.current &&
          move(dial.current, e) &&
          !e.currentTarget.hasPointerCapture(e.pointerId)
        )
          e.currentTarget.setPointerCapture(e.pointerId);
      },
      onPointerUp: () => dial.current && up(dial.current),
      onPointerCancel: () => dial.current && up(dial.current),
      onClickCapture: (e: MouseEvent) => {
        if (!dial.current?.blockClick) return;
        e.preventDefault();
        e.stopPropagation();
      },
      onKeyDown: (e: KeyboardEvent) => {
        if (dial.current && key(dial.current, e, labels)) e.preventDefault();
      },
    },
  };
}
