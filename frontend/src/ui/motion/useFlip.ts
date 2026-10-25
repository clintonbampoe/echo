import { useState } from "react";

// Changes every time `value` changes, and is undefined until the first change. Put it on an element as data-flip
// and the stylesheet (motion.css) replays the liquid squish each time: two animation names, swapped.
export function useFlip(value: string) {
  const [seen, setSeen] = useState(value);
  const [flip, setFlip] = useState<"a" | "b" | undefined>(undefined);
  if (seen !== value) {
    setSeen(value);
    setFlip(flip === "a" ? "b" : "a");
  }
  return flip;
}
