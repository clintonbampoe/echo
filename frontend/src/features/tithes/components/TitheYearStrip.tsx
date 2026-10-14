import { thisYear } from "@/lib/dates";
import { CellStrip, type Cell } from "@/ui";
import { useTitheMonths } from "../hooks";
import { titheMonths, yearSlots, type YearSlot } from "../view";

const toCell = (s: YearSlot, year: number): Cell => ({
  id: String(s.month),
  name: `${s.name} ${year}: ${s.state === "on" ? "given" : s.state === "off" ? "not given" : "not reached yet"}`,
  state: s.state,
  caption: s.name.slice(0, 1),
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
  const months = useTitheMonths(memberId, from, to);
  const rows = months.data ?? [];
  const { given, elapsed } = titheMonths(rows, today);

  return (
    <CellStrip
      label="Tithes this year"
      title={`Given in ${year}`}
      summary={`${given} of ${elapsed} months`}
      cells={yearSlots(rows, today).map((s) => toCell(s, year))}
      slots={12}
      status={
        months.isPending ? "loading" : months.isError ? "failed" : "ready"
      }
    />
  );
}
