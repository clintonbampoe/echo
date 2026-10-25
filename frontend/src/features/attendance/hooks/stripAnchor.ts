import { useState } from "react";
import { needsNewAnchor } from "../model/day";

// The day the strip is built around. It stays put while the picked day moves nearby, so the row is not rebuilt under a scroll.
export function useStripAnchor(day: string) {
  const [anchor, setAnchor] = useState(day);
  if (needsNewAnchor(anchor, day)) setAnchor(day);
  return anchor;
}
