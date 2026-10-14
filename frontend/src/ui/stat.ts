import type { Stat } from "./StatStrip";

// A number that could not be loaded says so instead of sitting on a dash.
export const failedStat = (stat: Stat): Stat => ({
  ...stat,
  value: "–",
  note: "Could not load",
});
