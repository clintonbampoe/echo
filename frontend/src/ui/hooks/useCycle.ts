import { useEffect, useState } from "react";

// Steps through a short list of words every few seconds, for small touches like a greeting that changes
// its wording or an example that changes its address. Reduced motion keeps the first item still.
export function useCycle(items: readonly string[], every = 3400): string {
  const [at, setAt] = useState(0);
  useEffect(() => {
    const still =
      typeof matchMedia === "function" &&
      matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (still || items.length < 2) return;
    const timer = setInterval(() => setAt((n) => n + 1), every);
    return () => clearInterval(timer);
  }, [items.length, every]);
  return items[at % items.length] ?? "";
}
