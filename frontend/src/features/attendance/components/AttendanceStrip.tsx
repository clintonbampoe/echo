import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { CellStrip, type Cell } from "@/ui";
import { RECENT } from "../api";
import type { Slot } from "../view";

type Props = {
  slots: Slot[];
  // What the sessions are called when one kind is chosen, e.g. "Sunday Service".
  typeName?: string;
  loading: boolean;
  failed: boolean;
  // How many squares: the latest sessions held.
  size?: number;
};

const say = (s: Slot) =>
  `${s.name}, ${formatDate(s.date)}: ${s.present ? "attended" : "missed"}`;
// The hover shows only which session; the square itself (ticked or empty) already says whether they came.
const toCell = (s: Slot): Cell => ({
  id: s.id,
  name: say(s),
  tip: `${s.name}, ${formatDate(s.date)}`,
  state: s.present ? "on" : "off",
  href: paths.session(s.id),
});

// The last few sessions as a row of squares, oldest to newest: ticked where the person was there, empty
// where they were not. Each square opens that session. Nothing at all when none have been held.
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
    <CellStrip
      label="Recent sessions"
      title={`Last ${size} ${typeName ? `${typeName} ` : ""}sessions`}
      summary={`${present} of ${slots.length} attended`}
      cells={slots.map(toCell)}
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
