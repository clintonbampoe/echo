import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { HistoryTrack, type TrackNode } from "@/ui";
import { RECENT } from "../../hooks";
import type { Slot } from "../../model/display";

type Props = {
  slots: Slot[];
  // What the sessions are called when one kind is chosen, e.g. "Sunday Service".
  typeName?: string;
  loading: boolean;
  failed: boolean;
  // How many nodes: the latest sessions held.
  size?: number;
};

const say = (s: Slot) =>
  `${s.name}, ${formatDate(s.date)}: ${s.present ? "attended" : "missed"}`;
// The hover shows only which session; the node itself (ticked or empty) already says whether they came.
const toNode = (s: Slot): TrackNode => ({
  id: s.id,
  name: say(s),
  tip: `${s.name}, ${formatDate(s.date)}`,
  state: s.present ? "on" : "off",
  href: paths.session(s.id),
});

// The last few sessions as a track of nodes, oldest to newest: ticked where the person was there, empty
// where they were not. Each node opens that session. Nothing at all when none have been held.
export function AttendanceStrip({
  slots,
  typeName,
  loading,
  failed,
  size = RECENT,
}: Props) {
  const waiting = loading || failed;
  if (!waiting && slots.length === 0) return null;
  const present = slots.filter((s) => s.present).length;
  const first = slots[0];
  const last = slots.at(-1);

  return (
    <HistoryTrack
      label="Recent sessions"
      title={`Last ${size} ${typeName ? `${typeName} ` : ""}sessions`}
      summary={`${present} of ${slots.length} attended`}
      nodes={slots.map(toNode)}
      slots={size}
      status={loading ? "loading" : failed ? "failed" : "ready"}
      ends={
        first && last
          ? [formatDate(first.date), formatDate(last.date)]
          : undefined
      }
    />
  );
}
