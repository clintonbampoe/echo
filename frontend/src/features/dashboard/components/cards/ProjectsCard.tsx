import { formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Aside, Chip, DashCard, DistributionBar, Figure } from "@/ui";
import { useProjects } from "../../hooks/projects";
import { CardFailed } from "./CardFailed";

const SHOWN = 4;

// What every project has raised, all time: the total, a bar cut by project, and the biggest ones with how far
// each is to its goal. The rest are gathered under one line.
export function ProjectsCard() {
  const { projects, failed, retry } = useProjects();
  const sorted = [...(projects ?? [])].sort((a, b) => b.raised - a.raised);
  const shown = sorted.slice(0, SHOWN);
  const rest = sorted.slice(SHOWN);
  const total = sorted.reduce((n, p) => n + p.raised, 0);
  const restRaised = rest.reduce((n, p) => n + p.raised, 0);
  return (
    <DashCard title="Projects" control={<Aside>of goal</Aside>}>
      {failed ? (
        <CardFailed onRetry={retry} />
      ) : (
        <>
          <Figure
            label="Raised so far"
            value={projects ? formatNumber(total) : "–"}
            unit="GHS raised"
          />
          <DistributionBar
            label="Raised by project"
            empty="Nothing raised yet"
            parts={[
              ...shown.map((p) => ({
                key: p.id,
                label: p.name,
                to: paths.project(p.id),
                value: p.raised,
                text: formatNumber(p.raised),
                share:
                  p.target > 0
                    ? `${Math.round((p.raised / p.target) * 100)}%`
                    : "",
                tag: p.atRisk ? <Chip tone="warning">At risk</Chip> : undefined,
              })),
              ...(rest.length > 0
                ? [
                    {
                      key: "rest",
                      label: `Other, ${rest.length} projects`,
                      value: restRaised,
                      text: formatNumber(restRaised),
                      closed: true,
                    },
                  ]
                : []),
            ]}
          />
        </>
      )}
    </DashCard>
  );
}
