import { formatNumber } from "@/lib/format";
import { StatStrip, type Stat } from "@/ui";
import type { MemberSummary } from "./api";

type Props = { summary?: MemberSummary };

const percent = (part: number, whole: number) =>
  whole > 0 ? Math.round((part / whole) * 100) : 0;

// The four numbers above the list. They follow the filters, so "Active" + "Accra" shows Accra's active members.
export function SummaryStrip({ summary: s }: Props) {
  const gendered = s ? s.femaleCount + s.maleCount : 0;
  const stats: Stat[] = [
    {
      label: "Members",
      value: s ? formatNumber(s.totalMembers) : "–",
      note: "All statuses",
    },
    {
      label: "Active",
      value: s ? formatNumber(s.activeMembers) : "–",
      note: s ? `${percent(s.activeMembers, s.totalMembers)}% of members` : "",
    },
    {
      label: "Women / Men",
      value: s
        ? `${percent(s.femaleCount, gendered)}% / ${percent(s.maleCount, gendered)}%`
        : "–",
      note: s
        ? `${formatNumber(s.femaleCount)} and ${formatNumber(s.maleCount)}`
        : "",
    },
    {
      label: "Average age",
      value: s ? String(Math.round(s.averageAge)) : "–",
      note: "Years",
    },
  ];
  return <StatStrip label="Member summary" stats={stats} busy={!s} />;
}
