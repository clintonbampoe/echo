import { useEffect, useState } from "react";

// A countdown in whole seconds, for a button that must wait before it can be used again. Returns the seconds
// left (0 when ready) and a function that starts it over. It counts to a time on the clock, not ticks, so the
// seconds spent in another app (a phone pauses the page) still count when the person comes back.
export function useCooldown(seconds: number, startNow = false) {
  const [end, setEnd] = useState(() =>
    startNow ? Date.now() + seconds * 1000 : 0,
  );
  const [left, setLeft] = useState(startNow ? seconds : 0);

  useEffect(() => {
    if (left <= 0) return;
    const sync = () =>
      setLeft(Math.max(0, Math.round((end - Date.now()) / 1000)));
    const timer = setTimeout(sync, 1000);
    document.addEventListener("visibilitychange", sync);
    return () => {
      clearTimeout(timer);
      document.removeEventListener("visibilitychange", sync);
    };
  }, [left, end]);

  const start = () => {
    setEnd(Date.now() + seconds * 1000);
    setLeft(seconds);
  };
  return [left, start] as const;
}
