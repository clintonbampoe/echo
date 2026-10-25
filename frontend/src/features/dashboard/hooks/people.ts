import { getMix } from "../api/members";
import { getRecentVisitors, getVisitorFigures } from "../api/visitors";
import { useData, useOnce } from "./internals";

// Visitors added in the range, and how many became members.
export function useVisitors(from: string, to: string) {
  const { data, failed, retry } = useData(
    "visitors",
    from,
    to,
    getVisitorFigures,
  );
  return { figures: data, failed, retry };
}

// How many members are men and women, and their age groups. One answer, shared by two cards.
export function useMix() {
  const { data, failed, retry } = useOnce("mix", getMix);
  return { mix: data, failed, retry };
}

// The newest visitors, over the widest window the card admits. The server sends a page in name order,
// so the sorting into newest first happens on the card, from the days the server said. No period.
export function useRecentVisitors(today: string) {
  const { data, failed, retry } = useData(
    "visitors-recent",
    today,
    today,
    (_f, t, signal) => getRecentVisitors(t, signal),
  );
  return { visitors: data, failed, retry };
}
