import { thisYear } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { HistoryTrack, type TrackNode } from "@/ui";
import { useMemberTithes, useTitheMonths } from "../../hooks/queries";
import { titheMonths, yearSlots, type YearSlot } from "../../model/display";

const toNode = (s: YearSlot, year: number, titheId?: string): TrackNode => ({
  id: String(s.month),
  name: `${s.name} ${year}: ${s.state === "on" ? "given" : s.state === "off" ? "not given" : "not reached yet"}`,
  tip: `${s.name.slice(0, 3)} ${year}`,
  state: s.state,
  caption: s.name.slice(0, 1),
  // A month they gave in opens that tithe.
  href: s.state === "on" && titheId ? paths.tithe(titheId) : undefined,
});

// This year at a glance, one node a month: ticked where they gave, empty where they did not.
export function TitheYearStrip({
  memberId,
  today,
}: {
  memberId: string;
  today: string;
}) {
  const { year, months: sofar, to } = thisYear(today);
  const months = useTitheMonths({ memberId }, sofar, to);
  const yearTithes = useMemberTithes(memberId, { year });
  const idOf = (month: string) =>
    yearTithes.rows.find((t) => t.forMonth === month && t.forYear === year)?.id;
  const rows = months.data ?? [];
  const { given, elapsed } = titheMonths(rows, today);

  return (
    <HistoryTrack
      label="Tithes this year"
      title={`Given in ${year}`}
      summary={`${given} of ${elapsed} months`}
      nodes={yearSlots(rows, today).map((s) => toNode(s, year, idOf(s.name)))}
      slots={12}
      status={
        months.isPending ? "loading" : months.isError ? "failed" : "ready"
      }
    />
  );
}
