import { formatNumber } from "@/lib/format";
import { useMix } from "../hooks";
import { genderRows } from "../people";
import { shareOf } from "../shares";
import { Breakdown } from "./Breakdown";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";

// Men, women and everyone else, across every member whatever their status. Not the period.
export function GenderCard() {
  const data = useMix();
  const ready = data.mix !== undefined;
  const { total, slices } = shareOf(genderRows(data.mix));
  return (
    <DashCard
      title="Gender"
      hint="All members, not the period"
      to="/members"
      foot={{
        note: "Counts include every status",
        to: "/members",
        label: "See members",
      }}
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
