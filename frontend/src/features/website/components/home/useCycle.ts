import { useEffect, useRef, useState } from "react";

const calm = () =>
  window.matchMedia?.("(prefers-reduced-motion: reduce)").matches ||
  document.documentElement.dataset.reduceMotion === "true";

// Steps through `count` things on its own every `every` ms. Touching one chooses it and holds the turning
// for `pause` ms; `hold(true)` stops it while the pointer rests on the thing. Nothing turns for people who
// ask for less motion.
export function useCycle(count: number, every: number) {
  const [index, setIndex] = useState(0);
  const until = useRef(0);
  const held = useRef(false);
  useEffect(() => {
    if (calm()) return;
    const timer = setInterval(() => {
      if (held.current || Date.now() < until.current) return;
      setIndex((i) => (i + 1) % count);
    }, every);
    return () => clearInterval(timer);
  }, [count, every]);
  return {
    index,
    choose: (next: number, pause: number) => {
      until.current = Date.now() + pause;
      setIndex(next);
    },
    hold: (on: boolean) => {
      held.current = on;
    },
  };
}
