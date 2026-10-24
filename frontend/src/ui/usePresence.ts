import { useEffect, useRef, useState } from "react";

// Switch for exit animations. The test setup turns them off so a closed dialog is gone at once; the tests of
// the animations themselves turn them back on.
export const exitAnimations = { enabled: true };

// How long an exit animation gets. None for a person who asked for less motion, or where there is nothing to
// animate (no matchMedia, or the tests).
export function exitDuration(ms: number) {
  if (!exitAnimations.enabled || typeof window.matchMedia !== "function")
    return 0;
  return window.matchMedia("(prefers-reduced-motion: reduce)").matches ? 0 : ms;
}

// Lets something play its exit animation before it leaves the page. `mounted` says whether to render it;
// `closing` says it is on its way out, so put data-closing on it and let the CSS animate. Pass the length of the
// exit animation as `ms`.
export function usePresence(open: boolean, ms = 200) {
  const [shown, setShown] = useState(open);
  if (open && !shown) setShown(true);

  useEffect(() => {
    if (open || !shown) return;
    const timer = window.setTimeout(() => setShown(false), exitDuration(ms));
    return () => window.clearTimeout(timer);
  }, [open, shown, ms]);

  // With no exit animation to play, it is gone the moment it closes.
  const leaving = !open && shown && exitDuration(ms) > 0;
  return { mounted: open || leaving, closing: leaving };
}

// For a dialog that is mounted only while it is wanted, so it has no `open` to hang an exit on: `leave(done)`
// marks it leaving (put data-closing on it), waits for the exit animation, then calls `done`.
export function useLeave(ms = 160) {
  const [leaving, setLeaving] = useState(false);
  const timer = useRef<number | undefined>(undefined);
  useEffect(() => () => window.clearTimeout(timer.current), []);

  function leave(done: () => void) {
    const wait = exitDuration(ms);
    if (wait === 0) return done();
    if (leaving) return;
    setLeaving(true);
    timer.current = window.setTimeout(done, wait);
  }
  return { leaving, leave };
}
