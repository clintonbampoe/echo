import { formatMoney } from "@/lib/format";
import { useProjects } from "../hooks";
import { shareOf } from "../shares";
import { Breakdown } from "./Breakdown";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";

type Props = {
  // The tall narrow card of a desktop row puts the legend under the ring.
  stacked?: boolean;
};

// Money raised so far for each project, all time (not the period picked above).
export function ProjectsCard({ stacked = false }: Props) {
  const data = useProjects();
  const ready = data.projects !== undefined;
  const { total, slices } = shareOf(
    (data.projects ?? []).map((p) => ({
      key: p.id,
      name: p.name,
      total: p.raised,
    })),
  );
  return (
    <DashCard
      title="Projects"
      hint="Raised so far, all time"
      to="/projects"
      foot={{
        note: "All time, not the period",
        to: "/projects",
        label: "See projects",
      }}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <Breakdown
          label="Raised by project"
          total={ready ? formatMoney(total) : "–"}
          totalLabel="Total raised"
          note="Total raised, all time"
          stacked={stacked}
          slices={slices.map((s) => ({
            key: s.key,
            name: s.name,
            value: s.total,
            text: formatMoney(s.total),
            percent: s.percent,
            color: s.color,
          }))}
          ready={ready}
          empty="Nothing raised yet"
        />
      )}
    </DashCard>
  );
}
