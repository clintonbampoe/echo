import { useState } from "react";

// The latest value that was not null, kept after it goes back to null. A sheet that is closing still needs the
// thing it was showing, even though the page has already let go of it.
export function useHeld<T>(value: T | null): T | null {
  const [held, setHeld] = useState(value);
  if (value !== null && value !== held) setHeld(value);
  return value ?? held;
}
