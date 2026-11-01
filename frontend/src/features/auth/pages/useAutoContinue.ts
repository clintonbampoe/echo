import { useEffect, useState } from "react";

const TICK_MS = 100;

// A countdown to "go on by itself". It stops while the person hovers or focuses the card (they are reading
// or about to click) and for good when they say "Stay here".
export function useAutoContinue(ms: number) {
  const [left, setLeft] = useState(ms);
  const [paused, setPaused] = useState(false);
  const [stopped, setStopped] = useState(false);

  useEffect(() => {
    if (paused || stopped) return;
    const timer = setInterval(
      () => setLeft((l) => Math.max(0, l - TICK_MS)),
      TICK_MS,
    );
    return () => clearInterval(timer);
  }, [paused, stopped]);

  return {
    // True when the countdown ran out and the page should go on.
    done: left === 0 && !stopped,
    progress: 1 - left / ms,
    seconds: Math.ceil(left / 1000),
    stopped,
    pause: setPaused,
    stop: () => setStopped(true),
  };
}
