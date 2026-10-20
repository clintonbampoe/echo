import { formatNumber } from "@/lib/format";
import { useGender } from "../hooks";
import { genderRows } from "../people";
import { shareOf } from "../shares";
import { Breakdown } from "./Breakdown";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";

// Men, women and everyone else, across every member whatever their status. Not the period.
export function GenderCard() {
  const data = useGender();
  const ready = data.figures !== undefined;
  const { total, slices } = shareOf(genderRows(data.figures));
  return (
    <DashCard
      tall
      title="Gender"
      hint="All members, not the period"
      to="/members"
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <Breakdown
          label="Members by gender"
          total={ready ? formatNumber(total) : "–"}
          totalLabel="Total members"
          note="Total members"
          slices={slices.map((s) => ({
            key: s.key,
            name: s.name,
            value: s.total,
            text: formatNumber(s.total),
            percent: s.percent,
            color: s.color,
          }))}
          ready={ready}
          empty="No members yet"
        />
      )}
    </DashCard>
  );
}
