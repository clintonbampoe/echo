import { thisYear } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { CellStrip, type Cell } from "@/ui";
import { useMemberTithes, useTitheMonths } from "../../hooks/queries";
import { titheMonths, yearSlots, type YearSlot } from "../../model/display";

const toCell = (s: YearSlot, year: number, titheId?: string): Cell => ({
  id: String(s.month),
  name: `${s.name} ${year}: ${s.state === "on" ? "given" : s.state === "off" ? "not given" : "not reached yet"}`,
  tip: `${s.name.slice(0, 3)} ${year}`,
  state: s.state,
  caption: s.name.slice(0, 1),
  // A month they gave in opens that tithe.
  href: s.state === "on" && titheId ? paths.tithe(titheId) : undefined,
});

// This year at a glance, one square a month: ticked where they gave, empty where they did not.
export function TitheYearStrip({
  memberId,
  today,
}: {
  memberId: string;
  today: string;
}) {
  const { year, from, to } = thisYear(today);
  const months = useTitheMonths({ memberId }, from, to);
  const yearTithes = useMemberTithes(memberId, { year });
  const idOf = (month: string) =>
    yearTithes.rows.find((t) => t.forMonth === month && t.forYear === year)?.id;
  const rows = months.data ?? [];
  const { given, elapsed } = titheMonths(rows, today);

  return (
    <CellStrip
      label="Tithes this year"
      title={`Given in ${year}`}
      summary={`${given} of ${elapsed} months`}
      cells={yearSlots(rows, today).map((s) => toCell(s, year, idOf(s.name)))}
      slots={12}
      status={
        months.isPending ? "loading" : months.isError ? "failed" : "ready"
      }
    />
  );
}
