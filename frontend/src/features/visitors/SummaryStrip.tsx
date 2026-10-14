import { formatNumber } from "@/lib/format";
import { StatStrip, type Stat } from "@/ui";
import type { VisitorSummary } from "./api";

type Props = { summary?: VisitorSummary; windowLabel: string };

// The four numbers above the list. They follow the time range, not the status or the search.
export function SummaryStrip({ summary: s, windowLabel }: Props) {
  const n = (value: number | undefined) =>
    s && value !== undefined ? formatNumber(value) : "–";
  const stats: Stat[] = [
    { label: "Visitors", value: n(s?.totalVisitors), note: "Not yet members" },
    {
      label: "Added",
      value: n(s?.newVisitors),
      note: `${windowLabel}, including those who became members`,
    },
    {
      label: "Returning",
      value: n(s?.recurringVisitors),
      note: "Came more than once",
    },
    {
      label: "Became members",
      value: n(s?.convertedVisitors),
      note: windowLabel,
    },
  ];
  return <StatStrip label="Visitor summary" stats={stats} busy={!s} />;
}
