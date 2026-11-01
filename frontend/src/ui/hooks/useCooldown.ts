import { useEffect, useState } from "react";

// A countdown in whole seconds, for a button that must wait before it can be used again. Returns the seconds
// left (0 when ready) and a function that starts it over.
export function useCooldown(seconds: number) {
  const [left, setLeft] = useState(0);

  useEffect(() => {
    if (left <= 0) return;
    const timer = setTimeout(() => setLeft(left - 1), 1000);
    return () => clearTimeout(timer);
  }, [left]);

  return [left, () => setLeft(seconds)] as const;
}
