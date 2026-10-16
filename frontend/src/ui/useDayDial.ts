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
  dispose,
  down,
  move,
  newDial,
  place,
  step,
  up,
  wheel,
  type Dial,
} from "./createDial";

// Wires the day strip's dial to a component: the picked day sits in the middle slot, and the row moves under a
// finger, the mouse, the wheel or the arrow keys with one continuous motion. Returns the handlers for the row.
export function useDayDial(
  track: RefObject<HTMLOListElement | null>,
  days: string[],
  selected: string,
  onSelect: (day: string) => void,
) {
  const latest = useRef({ days, selected, onSelect });
  const dial = useRef<Dial | null>(null);
  const first = days[0] ?? null;
  const index = days.indexOf(selected);
  // Before anything below reads it, so a rebuilt row is placed against its new days, not the old ones.
  useLayoutEffect(() => {
    latest.current = { days, selected, onSelect };
  });
  // Made when the strip mounts, so nothing about it is read during a render.
  useLayoutEffect(() => {
    const made = newDial(track, latest);
    dial.current = made;
    return () => dispose(made);
  }, [track]);
  useLayoutEffect(() => {
    if (dial.current) place(dial.current, index, first);
  }, [index, first]);
  // A wheel needs a listener that may cancel the page scroll.
  useEffect(() => {
    const el = track.current;
    if (!el) return;
    const onWheel = (e: WheelEvent) => {
      if (dial.current && wheel(dial.current, e)) e.preventDefault();
    };
    el.addEventListener("wheel", onWheel, { passive: false });
    return () => el.removeEventListener("wheel", onWheel);
  }, [track]);
  return {
    onPointerDown: (e: PointerEvent<HTMLOListElement>) =>
      dial.current && down(dial.current, e),
    onPointerMove: (e: PointerEvent<HTMLOListElement>) => {
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
      if (!dial.current || (e.key !== "ArrowLeft" && e.key !== "ArrowRight"))
        return;
      e.preventDefault();
      step(dial.current, e.key === "ArrowLeft" ? -1 : 1);
    },
  };
}
