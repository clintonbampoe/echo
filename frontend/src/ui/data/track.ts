// One node. on: came, or gave (an ink node with a check). off: did not (a ring with a dot). future: not reached yet
// (a dashed ring).
export type TrackNode = {
  id: string;
  // Said aloud, e.g. "Sunday Service, 11 Oct 2026: attended".
  name: string;
  state: "on" | "off" | "future";
  // The words in the pill on hover, e.g. "Sunday Service, 11 Oct 2026" (the screen reader still gets `name`).
  tip?: string;
  // When given, the node opens it.
  href?: string;
  // A letter under the node, e.g. the month's initial.
  caption?: string;
};

// A phone has room for six nodes; a tablet and a desktop show all twelve. Less, not smaller.
export const PHONE_NODES = 6;

// The nodes a phone shows: the six that end at the latest one reached, so a year does not open on months to come.
export function lastReached(nodes: TrackNode[], count = PHONE_NODES) {
  const reached = nodes.findLastIndex((n) => n.state !== "future") + 1;
  const end = reached || nodes.length;
  return nodes.slice(Math.max(0, end - count), end);
}
